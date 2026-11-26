using Code.Game.StaticData.Configs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Meta.UI.Common
{
    /// <summary>
    /// Pixel-art cloud veil (Tiny Swords style) over a map-like content: the world map, the research tree.
    /// Covers the whole content,
    /// leaves clearings where it is told to, and gently breathes while the map is open.
    /// Passive: no game logic here.
    /// </summary>
    public sealed class CloudVeil : VisualElement
    {
        public const int MaxHoles = 64;

        // The clearing field is smooth, a coarse texture is enough for it.
        private const float FieldStep = 8f;
        // The breathing veil (and an opening clearing) is redrawn at about 30 fps.
        private const long FrameMs = 16;
        private const float VeilFrame = 1f / 30f;
        private const int FieldPass = 0;
        private const int CloudPass = 1;

        private static readonly int HolesId = Shader.PropertyToID("_Holes");
        private static readonly int HoleCountId = Shader.PropertyToID("_HoleCount");
        private static readonly int MapSizeId = Shader.PropertyToID("_MapSize");
        private static readonly int BorderId = Shader.PropertyToID("_BorderWidth");
        private static readonly int FieldTexId = Shader.PropertyToID("_FieldTex");
        private static readonly int PuffSizeId = Shader.PropertyToID("_PuffSize");
        private static readonly int ShadowOffsetId = Shader.PropertyToID("_ShadowOffset");
        private static readonly int LightId = Shader.PropertyToID("_LightColor");
        private static readonly int MidId = Shader.PropertyToID("_MidColor");
        private static readonly int DarkId = Shader.PropertyToID("_DarkColor");
        private static readonly int ShadowId = Shader.PropertyToID("_ShadowColor");
        private static readonly int TimeId = Shader.PropertyToID("_CloudTime");
        private static readonly int BreathAmountId = Shader.PropertyToID("_BreathAmount");
        private static readonly int BreathSpeedId = Shader.PropertyToID("_BreathSpeed");

        private readonly Vector4[] _holes = new Vector4[MaxHoles];
        private readonly IVisualElementScheduledItem _tick;

        private int _holeCount;
        private Vector2 _mapSize;
        private CloudVeilSettings _settings;
        private Material _material;
        private RenderTexture _field;
        private RenderTexture _clouds;
        private bool _fieldDirty = true;
        private float _lastVeilTime = -1f;

        public CloudVeil()
        {
            pickingMode = PickingMode.Ignore;
            // Fills its parent; set here so the element works in any screen without extra USS.
            style.position = Position.Absolute;
            style.left = 0f;
            style.top = 0f;
            style.right = 0f;
            style.bottom = 0f;
            style.backgroundSize = new BackgroundSize(Length.Percent(100f), Length.Percent(100f));

            var shader = Shader.Find("Project2026/CloudVeil");

            if (shader != null)
                _material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };

            _tick = schedule.Execute(Tick).Every(FrameMs);
            _tick.Pause();
        }

        public void SetSettings(CloudVeilSettings settings)
        {
            _settings = settings;
            style.display = settings != null ? DisplayStyle.Flex : DisplayStyle.None;
            _fieldDirty = true;
            Redraw();
        }

        public void SetMapSize(Vector2 size)
        {
            if (_mapSize == size)
                return;

            _mapSize = size;
            _fieldDirty = true;
            Redraw();
        }

        /// <summary>Holes are (x, y, radius, scale 0..1). Animated: redrawn on the next frame, not now.</summary>
        public void SetHoles(List<Vector4> holes, bool animated = false)
        {
            _holeCount = Mathf.Min(holes.Count, MaxHoles);

            for (var i = 0; i < _holeCount; i++)
                _holes[i] = holes[i];

            _fieldDirty = true;

            if (!animated)
                Redraw();

        }

        public void SetRunning(bool running)
        {
            if (running)
                _tick.Resume();
            else
                _tick.Pause();
        }

        public void Dispose()
        {
            _tick.Pause();
            style.backgroundImage = new StyleBackground();
            Release(ref _field);
            Release(ref _clouds);

            if (_material != null)
            {
                Object.Destroy(_material);
                _material = null;
            }
        }

        private void Tick(TimerState timer)
        {
            var now = Time.unscaledTime;

            // A clearing is opening (reveal animation) or the edge breathes: redraw at the veil rate.
            var animated = _fieldDirty || (_settings != null && _settings.BreathAmount > 0f);

            if (animated && now - _lastVeilTime >= VeilFrame)
                Redraw();
        }

        private void Redraw()
        {
            if (_material == null || _settings == null || _mapSize.x <= 0f || _mapSize.y <= 0f)
                return;

            var pixel = Mathf.Max(1f, _settings.PixelSize);
            var created = Ensure(ref _field, Size(FieldStep), RenderTextureFormat.RHalf, FilterMode.Bilinear);
            created |= Ensure(ref _clouds, Size(pixel), RenderTextureFormat.ARGB32, FilterMode.Point);

            _material.SetVector(MapSizeId, new Vector4(_mapSize.x, _mapSize.y, 0f, 0f));
            _material.SetFloat(BorderId, _settings.BorderWidth);

            if (_fieldDirty || created)
            {
                _material.SetVectorArray(HolesId, _holes);
                _material.SetFloat(HoleCountId, _holeCount);
                Graphics.Blit(Texture2D.whiteTexture, _field, _material, FieldPass);
                _fieldDirty = false;
            }

            _material.SetTexture(FieldTexId, _field);
            _material.SetFloat(PuffSizeId, Mathf.Max(8f, _settings.PuffSize));
            _material.SetFloat(ShadowOffsetId, _settings.ShadowOffset);
            _material.SetColor(LightId, _settings.Light);
            _material.SetColor(MidId, _settings.Mid);
            _material.SetColor(DarkId, _settings.Dark);
            _material.SetColor(ShadowId, _settings.ShadowColor);
            // Wrapped so the sine phases keep their float precision in long sessions.
            _material.SetFloat(TimeId, Time.unscaledTime % 3600f);
            _material.SetFloat(BreathAmountId, _settings.BreathAmount);
            _material.SetFloat(BreathSpeedId, _settings.BreathSpeed);
            _lastVeilTime = Time.unscaledTime;
            Graphics.Blit(Texture2D.whiteTexture, _clouds, _material, CloudPass);

            if (created)
                style.backgroundImage = Background.FromRenderTexture(_clouds);
            else
                MarkDirtyRepaint();
        }

        private Vector2Int Size(float step) => new(
            Mathf.Max(1, Mathf.RoundToInt(_mapSize.x / step)),
            Mathf.Max(1, Mathf.RoundToInt(_mapSize.y / step)));

        // ---- Render textures ----

        private static bool Ensure(ref RenderTexture texture, Vector2Int size, RenderTextureFormat format, FilterMode filter)
        {
            if (texture != null && texture.width == size.x && texture.height == size.y)
                return false;

            Release(ref texture);

            if (!SystemInfo.SupportsRenderTextureFormat(format))
                format = RenderTextureFormat.ARGBHalf;

            texture = new RenderTexture(size.x, size.y, 0, format)
            {
                filterMode = filter,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.Create();
            return true;
        }

        private static void Release(ref RenderTexture texture)
        {
            if (texture == null)
                return;

            texture.Release();
            Object.Destroy(texture);
            texture = null;
        }
    }
}
