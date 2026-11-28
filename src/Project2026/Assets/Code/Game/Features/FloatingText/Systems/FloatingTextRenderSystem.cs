using Code.Game.Common.Render;
using Code.Game.StaticData.Configs;
using Entitas;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Code.Game.Features.FloatingText.Systems
{
    /// <summary>
    /// Draws every floating number (damage) as SDF glyph quads from the TextMeshPro font atlas into one batched
    /// mesh: all numbers are a single draw call, no TextMeshPro GameObject or mesh rebuild per number.
    /// Same motion as the old FloatingTextView: rise with OutQuad, fade with InQuad, centered on the spawn point.
    /// Digits are laid out from the int directly (no strings), the glyph table is read from the font once.
    /// </summary>
    public class FloatingTextRenderSystem : IExecuteSystem, ITearDownSystem
    {
        private const int MaxGlyphsPerText = 12; // sign + int digits
        private const int MinusGlyph = 10;
        private const int PlusGlyph = 11;
        private const string Characters = "0123456789-+";

        private struct GlyphInfo
        {
            public bool Valid;
            public Vector4 Uv;      // uMin, vMin, uMax, vMax (padded)
            public float Left;      // quad left from the pen, world units (padded)
            public float Bottom;    // quad bottom from the baseline, world units (padded)
            public float Width;     // quad size, world units (padded)
            public float Height;
            public float Advance;
        }

        private readonly BattleOverlayConfig _config;
        private readonly IGroup<GameEntity> _texts;
        private readonly List<GameEntity> _buffer = new(256);
        private readonly GlyphInfo[] _glyphs = new GlyphInfo[Characters.Length];

        private BatchedQuadMesh _mesh;
        private Material _material;
        private bool _ownsMaterial;
        private float _baselineOffset;

        public FloatingTextRenderSystem(BattleOverlayConfig config)
        {
            _config = config;
            _texts = Contexts.sharedInstance.game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.FloatingText,
                    GameMatcher.FloatingTextAge,
                    GameMatcher.SpawnPosition));
        }

        public void Execute()
        {
            if (!EnsureMesh())
                return;

            var texts = _texts.GetEntities(_buffer);
            var duration = Mathf.Max(0.0001f, _config.Duration);
            var floatDistance = _config.FloatDistance;

            Span<byte> digits = stackalloc byte[MaxGlyphsPerText];

            _mesh.Begin();

            for (var i = 0; i < texts.Count; i++)
            {
                var text = texts[i];

                if (text.isDestructed)
                    continue;

                var progress = Mathf.Clamp01(text.floatingTextAge.Value / duration);
                var rise = floatDistance * (1f - ((1f - progress) * (1f - progress))); // OutQuad
                var color = text.hasFloatingTextColor ? text.floatingTextColor.Value : Color.white;
                color.a *= 1f - (progress * progress); // InQuad fade to 0

                var origin = text.spawnPosition.Value;
                var count = Layout(text.floatingText.Value, digits);

                if (!DrawNumber(digits.Slice(0, count), origin.x, origin.y + rise + _baselineOffset, color))
                    break; // full: drop the rest this frame
            }

            _mesh.End();
        }

        public void TearDown()
        {
            _mesh?.Dispose();
            _mesh = null;

            if (_ownsMaterial && _material != null)
                UnityEngine.Object.Destroy(_material);

            _material = null;
            _ownsMaterial = false;
        }

        /// <summary>Writes glyph indices of <paramref name="value"/> ("-12", "7") into <paramref name="output"/>.</summary>
        private static int Layout(int value, Span<byte> output)
        {
            var count = 0;
            var negative = value < 0;
            // long: -int.MinValue overflows int.
            var magnitude = negative ? -(long)value : value;

            Span<byte> reversed = stackalloc byte[MaxGlyphsPerText];
            var digitCount = 0;

            do
            {
                reversed[digitCount++] = (byte)(magnitude % 10);
                magnitude /= 10;
            }
            while (magnitude > 0 && digitCount < MaxGlyphsPerText - 1);

            if (negative)
                output[count++] = MinusGlyph;

            for (var i = digitCount - 1; i >= 0; i--)
                output[count++] = reversed[i];

            return count;
        }

        private bool DrawNumber(ReadOnlySpan<byte> glyphs, float centerX, float baselineY, Color32 color)
        {
            var width = 0f;

            for (var i = 0; i < glyphs.Length; i++)
                width += _glyphs[glyphs[i]].Advance;

            var pen = centerX - (width * 0.5f);

            for (var i = 0; i < glyphs.Length; i++)
            {
                ref readonly var glyph = ref _glyphs[glyphs[i]];

                if (glyph.Valid)
                {
                    var x0 = pen + glyph.Left;
                    var y0 = baselineY + glyph.Bottom;

                    if (!_mesh.TryAdd(x0, y0, x0 + glyph.Width, y0 + glyph.Height, color, glyph.Uv))
                        return false;
                }

                pen += glyph.Advance;
            }

            return true;
        }

        private bool EnsureMesh()
        {
            if (_mesh != null)
                return true;

            if (_config == null || _config.DigitsMaterial == null || _config.Font == null)
                return false;

            BuildGlyphs(_config.Font);

            _material = _config.DigitsMaterial;

            // The material must sample this font's atlas; use an instance if the asset points elsewhere.
            if (_material.mainTexture != _config.Font.atlasTexture)
            {
                _material = new Material(_material) { hideFlags = HideFlags.DontSave, mainTexture = _config.Font.atlasTexture };
                _ownsMaterial = true;
            }

            _mesh = new BatchedQuadMesh(
                "DamageNumbers",
                _material,
                _config.TextSortingOrder,
                Mathf.Max(1, _config.MaxTexts) * MaxGlyphsPerText);

            return true;
        }

        private void BuildGlyphs(TMP_FontAsset font)
        {
            var face = font.faceInfo;
            // TextMeshPro (3D) scale: font size / sampling point size * face scale * 0.1.
            var scale = _config.FontSize / face.pointSize * face.scale * 0.1f;
            var atlasWidth = (float)font.atlasWidth;
            var atlasHeight = (float)font.atlasHeight;
            var pad = _config.GlyphPadding;

            // Vertical "Middle" alignment: center the line between ascent and descent.
            _baselineOffset = -(face.ascentLine + face.descentLine) * 0.5f * scale;

            for (var i = 0; i < Characters.Length; i++)
            {
                if (!font.characterLookupTable.TryGetValue(Characters[i], out var character) || character.glyph == null)
                {
                    _glyphs[i] = default;
                    continue;
                }

                var glyph = character.glyph;
                var rect = glyph.glyphRect;
                var metrics = glyph.metrics;

                _glyphs[i] = new GlyphInfo
                {
                    Valid = rect.width > 0 && rect.height > 0,
                    Uv = new Vector4(
                        (rect.x - pad) / atlasWidth,
                        (rect.y - pad) / atlasHeight,
                        (rect.x + rect.width + pad) / atlasWidth,
                        (rect.y + rect.height + pad) / atlasHeight),
                    Left = (metrics.horizontalBearingX - pad) * scale,
                    Bottom = (metrics.horizontalBearingY - metrics.height - pad) * scale,
                    Width = (metrics.width + (pad * 2f)) * scale,
                    Height = (metrics.height + (pad * 2f)) * scale,
                    Advance = metrics.horizontalAdvance * scale,
                };
            }
        }
    }
}
