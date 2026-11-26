using Code.Game.StaticData.Configs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Meta.UI.MainMenu
{
    /// <summary>
    /// Pixel-art cloud veil over the world map (Tiny Swords style). Covers the whole map content,
    /// leaves clearings where it is told to, and floats a few of the pack's cloud sprites over the veil.
    /// Passive: no game logic here.
    /// </summary>
    public sealed class LevelMapClouds : VisualElement
    {
        public const int MaxHoles = 64;

        // The clearing field is smooth, a coarse texture is enough for it.
        private const float FieldStep = 8f;
        // Drifting sprites move every frame; the breathing veil is redrawn at about 30 fps.
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
        private readonly List<DriftCloud> _drifters = new(8);
        private readonly IVisualElementScheduledItem _tick;

        private int _holeCount;
        private Vector2 _mapSize;
        private LevelMapVeilSettings _settings;
        private Material _material;
        private RenderTexture _field;
        private RenderTexture _clouds;
        private bool _fieldDirty = true;
        private float _lastTime = -1f;
        private float _lastVeilTime = -1f;

        public LevelMapClouds()
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList("level-map-clouds");
            style.backgroundSize = new BackgroundSize(Length.Percent(100f), Length.Percent(100f));

            var shader = Shader.Find("Project2026/LevelMapClouds");

            if (shader != null)
                _material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };

            _tick = schedule.Execute(Tick).Every(FrameMs);
            _tick.Pause();
        }

        public void SetSettings(LevelMapVeilSettings settings)
        {
            _settings = settings;
            style.display = settings != null ? DisplayStyle.Flex : DisplayStyle.None;
            _fieldDirty = true;
            BuildDrifters();
            Redraw();
        }

        public void SetMapSize(Vector2 size)
        {
            if (_mapSize == size)
                return;

            _mapSize = size;
            _fieldDirty = true;
            BuildDrifters();
            Redraw();
        }

        public void SetHoles(List<Vector4> holes)
        {
            _holeCount = Mathf.Min(holes.Count, MaxHoles);

            for (var i = 0; i < _holeCount; i++)
                _holes[i] = holes[i];

            _fieldDirty = true;
            Redraw();
            UpdateDrifters(0f);
        }

        public void SetRunning(bool running)
        {
            _lastTime = -1f;

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
            var dt = _lastTime < 0f ? 0f : Mathf.Min(now - _lastTime, 0.1f);
            _lastTime = now;

            UpdateDrifters(dt);

            if (_settings != null && _settings.BreathAmount > 0f && now - _lastVeilTime >= VeilFrame)
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

        // ---- Drifting sprites ----

        private void BuildDrifters()
        {
            for (var i = 0; i < _drifters.Count; i++)
                _drifters[i].View.RemoveFromHierarchy();

            _drifters.Clear();

            if (_settings == null || _mapSize.x <= 0f || _settings.DriftClouds == null || _settings.DriftClouds.Length == 0)
                return;

            var random = new System.Random(2026);
            var scale = _settings.PixelSize * _settings.DriftCloudScale;
            var textures = _settings.DriftClouds;

            for (var i = 0; i < _settings.DriftCloudCount; i++)
            {
                var texture = textures[i % textures.Length];

                if (texture == null)
                    continue;

                var size = new Vector2(texture.width, texture.height) * scale;
                var view = new VisualElement { pickingMode = PickingMode.Ignore };
                view.AddToClassList("level-map-drift-cloud");
                view.style.width = size.x;
                view.style.height = size.y;
                view.style.backgroundImage = texture;
                view.style.opacity = 0f;
                Add(view);

                _drifters.Add(new DriftCloud(view, size)
                {
                    Position = new Vector2(
                        (float)random.NextDouble() * (_mapSize.x + size.x) - size.x,
                        (float)random.NextDouble() * (_mapSize.y - size.y * 0.5f) - size.y * 0.25f)
                });
            }

            UpdateDrifters(0f);
        }

        private void UpdateDrifters(float dt)
        {
            if (_settings == null)
                return;

            for (var i = 0; i < _drifters.Count; i++)
            {
                var drifter = _drifters[i];
                drifter.Position.x += _settings.DriftSpeed * dt;

                if (drifter.Position.x > _mapSize.x)
                    drifter.Position.x = -drifter.Size.x;

                // Sub-pixel position: snapping to the art grid at this speed reads as stutter.
                drifter.View.style.translate = new Translate(drifter.Position.x, drifter.Position.y);

                var visible = OverVeil(drifter);

                if (visible != drifter.Visible)
                {
                    drifter.Visible = visible;
                    drifter.View.style.opacity = visible ? 1f : 0f;
                }
            }
        }

        // The pack's cloud art fills roughly the middle of the texture; all of it must sit over the veil.
        private bool OverVeil(DriftCloud drifter)
        {
            var margin = _settings.PuffSize * 0.5f;
            var center = drifter.Position + drifter.Size * 0.5f;
            var half = new Vector2(drifter.Size.x * 0.35f, drifter.Size.y * 0.25f);

            return Clear(center) > margin
                && Clear(center + new Vector2(-half.x, -half.y)) > margin
                && Clear(center + new Vector2(half.x, -half.y)) > margin
                && Clear(center + new Vector2(-half.x, half.y)) > margin
                && Clear(center + new Vector2(half.x, half.y)) > margin;
        }

        // CPU twin of the shader's clearing field (without the smoothing): > 0 under the clouds.
        private float Clear(Vector2 point)
        {
            var distance = float.MaxValue;

            for (var i = 0; i < _holeCount; i++)
            {
                var hole = _holes[i];
                distance = Mathf.Min(distance, Vector2.Distance(point, new Vector2(hole.x, hole.y)) - hole.z);
            }

            var edge = Mathf.Min(Mathf.Min(point.x, _mapSize.x - point.x), Mathf.Min(point.y, _mapSize.y - point.y));
            return Mathf.Max(distance, _settings.BorderWidth - edge);
        }

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

        private sealed class DriftCloud
        {
            public readonly VisualElement View;
            public readonly Vector2 Size;
            public Vector2 Position;
            public bool Visible;

            public DriftCloud(VisualElement view, Vector2 size)
            {
                View = view;
                Size = size;
            }
        }
    }
}
