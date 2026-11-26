using Code.Game.Common.Entity;
using Code.Game.Common.UI;
using Code.Game.StaticData.Configs;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Code.Meta.UI.MainMenu
{
    /// <summary>
    /// Full-screen world map view. Draws what LevelMapRenderSystem tells it, pans by drag,
    /// and sends requests to the input context. No game logic here.
    /// </summary>
    public class LevelSelectMenu : MonoBehaviour
    {
        private static readonly Vector2 DefaultMapSize = new(2000f, 1200f);

        // A clear hole around a flag is centered above the pole base so the banner sits inside it.
        private const float HoleLift = 50f;

        // Lets the panel fade in before the reveal starts.
        private const float RevealDelay = 0.5f;
        private const int PuffCount = 6;

        [SerializeField] private UIDocument _mainMenuDoc;

        private UIService _uiService;

        private VisualElement _panel;
        private VisualElement _viewport;
        private VisualElement _content;
        private LevelMapBackdrop _backdrop;
        private LevelMapRoads _roads;
        private LevelMapClouds _clouds;
        private LevelMapVeilSettings _veil;
        private Label _starsValue;
        private Button _backButton;

        private readonly Dictionary<string, Marker> _markers = new(16);
        private readonly List<LevelMapLink> _links = new(16);
        private readonly List<float> _linkReveal = new(16);
        private readonly List<Vector4> _holes = new(LevelMapClouds.MaxHoles);
        private LevelMapRevealPlayer _player;
        private Vector2? _focusOverride;

        private Vector2 _mapSize = DefaultMapSize;
        private Vector2 _pan;
        private bool _focusPending;
        private bool _dragging;
        private int _dragPointer = -1;

        public event Action<string> LevelChosen;
        public event Action BackRequested;

        [Inject]
        private void Construct(UIService uiService)
        {
            _uiService = uiService;
        }

        private void Awake()
        {
            var root = _mainMenuDoc.rootVisualElement;

            _panel = root.Q<VisualElement>("LevelSelectPanel");
            _viewport = root.Q<VisualElement>("LevelMapViewport");
            _content = root.Q<VisualElement>("LevelMapContent");
            _starsValue = root.Q<Label>("LevelMapStarsValue");
            _backButton = root.Q<Button>("LevelSelectBackButton");

            BuildMap();
            BuildHud(root);

            if (_backButton != null)
                _backButton.clickable.clicked += OnBackClicked;

            _uiService.Hide(_panel).Forget();
        }

        public void Open() => CreateInputEntity.Empty().isLevelMapMenuOpenRequested = true;

        public void Close() => CreateInputEntity.Empty().isLevelMapMenuCloseRequested = true;

        public void Show()
        {
            _focusPending = true;
            _clouds?.SetRunning(true);
            _uiService.Show(_panel).Forget();
        }

        public void Hide()
        {
            StopDrag(_dragPointer);
            _player?.Complete();
            _clouds?.SetRunning(false);
            _uiService.Hide(_panel).Forget();
        }

        public void NotifyChosen(string nodeId) => LevelChosen?.Invoke(nodeId);

        public void SetMap(Vector2 size, Sprite background)
        {
            _mapSize = size.x > 0f && size.y > 0f ? size : DefaultMapSize;

            if (_content != null)
            {
                _content.style.width = _mapSize.x;
                _content.style.height = _mapSize.y;
            }

            _backdrop?.SetSprite(background);
            _clouds?.SetMapSize(_mapSize);
            ApplyPan();
        }

        public void SetVeil(LevelMapVeilSettings veil)
        {
            _veil = veil;
            _clouds?.SetSettings(veil);
        }

        public void SetNode(string id, string title, LevelNodeState state, LevelNodeState seenState, int stars, Vector2 position)
        {
            if (_content == null || string.IsNullOrEmpty(id))
                return;

            if (!_markers.TryGetValue(id, out var marker))
            {
                var view = new LevelMapMarker(() => CreateInputEntity.Empty().AddLevelNodeChooseRequested(id));
                _content.Add(view);
                marker = new Marker(view);
                _markers.Add(id, marker);
            }

            marker.State = state;
            marker.SeenState = seenState;
            marker.Title = title;
            marker.Stars = stars;
            marker.Position = position;
            marker.HoleScale = 1f;
            marker.Grow = 1f;
            marker.View.SetAppear(1f, 1f);
            marker.View.SetShake(0f);
            marker.View.Set(title, state, stars);
            marker.View.PlaceAt(position);
        }

        public void SetLinks(List<LevelMapLink> links)
        {
            _player?.Complete();
            _roads?.Set(links);
            _links.Clear();
            _links.AddRange(links);
            _linkReveal.Clear();

            for (var i = 0; i < links.Count; i++)
                _linkReveal.Add(1f);

            SortMarkers();
            PlayReveal();
            UpdateClouds();

            if (_focusPending)
                FocusOnFrontier();
        }

        public void SetStars(int earned, int max)
        {
            if (_starsValue != null)
                _starsValue.text = $"{earned}/{max}";
        }

        private void BuildMap()
        {
            if (_content == null || _viewport == null)
                return;

            _content.Clear();
            _content.style.width = _mapSize.x;
            _content.style.height = _mapSize.y;

            _backdrop = new LevelMapBackdrop();
            _content.Add(_backdrop);

            _roads = new LevelMapRoads();
            _content.Add(_roads);

            // Above flags and roads; markers are kept below it by SortMarkers.
            _clouds = new LevelMapClouds();
            _clouds.SetMapSize(_mapSize);
            _content.Add(_clouds);

            _player = new LevelMapRevealPlayer(_content);

            _viewport.RegisterCallback<PointerDownEvent>(OnPointerDown);
            _viewport.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _viewport.RegisterCallback<PointerUpEvent>(OnPointerUp);
            _viewport.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
            _viewport.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                if (_focusPending)
                    FocusOnFrontier();
                else
                    ApplyPan();
            });
        }

        private void BuildHud(VisualElement root)
        {
            var stars = root.Q<VisualElement>("LevelMapStars");

            if (stars == null)
                return;

            var icon = new LevelMapStar();
            icon.AddToClassList("level-map-hud__star");
            stars.Insert(0, icon);
        }

        // Backdrop and roads, then markers (further down the map drawn over the ones above), clouds on top.
        private void SortMarkers()
        {
            _content.Sort((a, b) =>
            {
                var aLayer = Layer(a);
                var bLayer = Layer(b);

                if (aLayer != bLayer)
                    return aLayer.CompareTo(bLayer);

                return aLayer == 1 ? a.style.top.value.value.CompareTo(b.style.top.value.value) : 0;
            });
        }

        private static int Layer(VisualElement element) =>
            element is LevelMapClouds ? 2 : element is LevelMapMarker ? 1 : 0;

        // Clear holes: around every visible flag first, then along visible roads.
        // While the reveal plays, flags and roads carry their animation progress in the hole scale.
        private void UpdateClouds(bool animated = false)
        {
            if (_clouds == null || _veil == null)
                return;

            _holes.Clear();

            foreach (var marker in _markers.Values)
            {
                if (marker.State == LevelNodeState.Hidden)
                    continue;

                var radius = marker.State == LevelNodeState.Locked
                    ? _veil.PreviewRadius
                    : Mathf.Lerp(_veil.PreviewRadius, _veil.RevealRadius, marker.Grow);

                AddHole(marker.Position + new Vector2(0f, -HoleLift), radius, marker.HoleScale);
            }

            var step = Mathf.Max(10f, _veil.RoadStep);

            for (var i = 0; i < _links.Count; i++)
            {
                var link = _links[i];
                var reveal = _linkReveal[i];
                var samples = Mathf.CeilToInt(Vector2.Distance(link.From, link.To) / step);

                for (var s = 1; s < samples; s++)
                {
                    // Clouds part along the road just ahead of the line being drawn.
                    var along = (float)s / samples;
                    var scale = reveal >= 1f ? 1f : Mathf.Clamp01((reveal - along + 0.15f) / 0.3f);
                    AddHole(LevelMapRoads.Point(link.From, link.To, along), _veil.RoadRadius, scale);
                }
            }

            _clouds.SetHoles(_holes, animated);
        }

        private void AddHole(Vector2 center, float radius, float scale)
        {
            if (_holes.Count < LevelMapClouds.MaxHoles)
                _holes.Add(new Vector4(center.x, center.y, radius, scale));
        }

        // ---- Reveal animation: plays the difference between what the player last saw and now ----

        private static bool IsVisible(LevelNodeState state) => state != LevelNodeState.Hidden;

        private static bool IsNewlyVisible(Marker marker) => !IsVisible(marker.SeenState) && IsVisible(marker.State);

        private void PlayReveal()
        {
            _focusOverride = null;

            if (_player == null)
                return;

            var time = RevealDelay;

            // 1. Flags already on the map that changed: the lock breaks, or the level got beaten.
            foreach (var marker in _markers.Values)
            {
                if (!IsVisible(marker.SeenState) || !IsVisible(marker.State) || marker.SeenState == marker.State)
                    continue;

                _focusOverride ??= marker.Position;
                time = PlayStateChange(marker, time);
            }

            // 2. New flags that no new road leads to (the very first level).
            var stagger = 0f;

            foreach (var marker in _markers.Values)
            {
                if (!IsNewlyVisible(marker) || HasFreshRoadTo(marker.Position))
                    continue;

                _focusOverride ??= marker.Position;
                PlayAppear(marker, time + stagger);
                stagger += 0.2f;
            }

            if (stagger > 0f)
                time += 0.8f + stagger;

            // 3. New roads draw in while the clouds part along them.
            var roads = 0f;

            for (var i = 0; i < _links.Count; i++)
            {
                if (!_links[i].Fresh)
                    continue;

                var index = i;
                var duration = Mathf.Clamp(Vector2.Distance(_links[i].From, _links[i].To) / 450f, 0.7f, 1.6f);
                _focusOverride ??= _links[i].To;
                roads = Mathf.Max(roads, duration);

                _player.Add(time, duration, p =>
                {
                    var eased = LevelMapRevealPlayer.EaseInOut(p);
                    _linkReveal[index] = eased;
                    _roads.SetProgress(index, eased);
                    UpdateClouds(true);
                });
            }

            time += roads * 0.75f;
            stagger = 0f;

            // 4. New flags at the end of the new roads.
            foreach (var marker in _markers.Values)
            {
                if (!IsNewlyVisible(marker) || !HasFreshRoadTo(marker.Position))
                    continue;

                PlayAppear(marker, time + stagger);
                stagger += 0.2f;
            }

            _player.Play();
        }

        private bool HasFreshRoadTo(Vector2 position)
        {
            for (var i = 0; i < _links.Count; i++)
            {
                if (_links[i].Fresh && _links[i].To == position)
                    return true;
            }

            return false;
        }

        // Clouds open over the flag, then it pops in.
        private void PlayAppear(Marker marker, float at)
        {
            marker.View.SetAnimating();

            _player.Add(at, 0.9f, p =>
            {
                marker.HoleScale = LevelMapRevealPlayer.EaseOut(p);
                UpdateClouds(true);
            });

            _player.Add(at + 0.45f, 0.4f,
                p => marker.View.SetAppear(Mathf.Clamp01(p * 2f), Mathf.LerpUnclamped(0.6f, 1f, LevelMapRevealPlayer.BackOut(p))),
                () => marker.View.Set(marker.Title, marker.State, marker.Stars));
        }

        // Shows the old look first. A lock shakes and breaks (with a puff of cloud) and the clearing
        // widens; any change ends with a little punch into the new look. Returns when the next may start.
        private float PlayStateChange(Marker marker, float at)
        {
            marker.View.Set(marker.Title, marker.SeenState, marker.Stars);
            marker.View.SetAnimating();

            if (marker.SeenState == LevelNodeState.Locked)
            {
                marker.Grow = 0f;
                _player.Add(at, 0.4f, p => marker.View.SetShake(Mathf.Sin(p * Mathf.PI * 6f) * 8f * (1f - p)));
                at += 0.4f;

                _player.Add(at, 1f, p =>
                {
                    marker.Grow = LevelMapRevealPlayer.EaseOut(p);
                    UpdateClouds(true);
                });

                var position = marker.Position;
                _player.At(at, () => SpawnPuffs(position));
            }

            _player.At(at, () =>
            {
                marker.View.Set(marker.Title, marker.State, marker.Stars);
                marker.View.SetAnimating();
            });

            _player.Add(at, 0.35f,
                p => marker.View.SetAppear(1f, 1f + 0.2f * Mathf.Sin(p * Mathf.PI)),
                () => marker.View.Set(marker.Title, marker.State, marker.Stars));

            return at + 0.3f;
        }

        // A few square bits of cloud fly off the banner and fade out (when a lock breaks).
        private void SpawnPuffs(Vector2 position)
        {
            if (_content == null || _clouds == null || _clouds.parent != _content)
                return;

            var pixel = _veil != null ? Mathf.Max(1f, _veil.PixelSize) : 2f;
            var light = _veil != null ? _veil.Light : Color.white;
            var mid = _veil != null ? _veil.Mid : Color.white;
            var origin = position + new Vector2(0f, -70f);
            var start = Time.unscaledTime;

            for (var i = 0; i < PuffCount; i++)
            {
                var size = pixel * (4 + i % 3);
                var angle = (i + 0.3f) / PuffCount * Mathf.PI * 2f;
                var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var from = origin - Vector2.one * size * 0.5f;

                var puff = new VisualElement { pickingMode = PickingMode.Ignore };
                puff.AddToClassList("level-map-puff");
                puff.style.width = size;
                puff.style.height = size;
                puff.style.backgroundColor = i % 2 == 0 ? light : mid;
                puff.style.translate = new Translate(from.x, from.y);
                _content.Insert(_content.IndexOf(_clouds), puff);

                // Own little schedule: the puffs run on their own clock, not the reveal timeline.
                puff.schedule.Execute(timer =>
                {
                    var p = Mathf.Clamp01((Time.unscaledTime - start) / 0.55f);
                    var offset = direction * (14f + 46f * LevelMapRevealPlayer.EaseOut(p));
                    puff.style.translate = new Translate(from.x + offset.x, from.y + offset.y);
                    puff.style.opacity = 1f - p * p;
                }).Every(16).Until(() =>
                {
                    if (Time.unscaledTime - start < 0.55f)
                        return false;

                    puff.RemoveFromHierarchy();
                    return true;
                });
            }
        }

        // Center the camera on the newest playable level (or the last beaten one).
        private void FocusOnFrontier()
        {
            var size = ViewportSize();

            if (size == Vector2.zero || _markers.Count == 0)
                return;

            Vector2? target = null;
            Vector2? fallback = null;

            foreach (var marker in _markers.Values)
            {
                if (marker.State == LevelNodeState.Available && (target == null || marker.Position.x > target.Value.x))
                    target = marker.Position;

                if (marker.State == LevelNodeState.Completed && (fallback == null || marker.Position.x > fallback.Value.x))
                    fallback = marker.Position;
            }

            var focus = _focusOverride ?? target ?? fallback ?? _mapSize * 0.5f;
            _pan = size * 0.5f - focus;
            _focusPending = false;
            ApplyPan();
        }

        private void ApplyPan()
        {
            if (_content == null)
                return;

            var size = ViewportSize();

            if (size == Vector2.zero)
                return;

            _pan.x = ClampAxis(_pan.x, size.x, _mapSize.x);
            _pan.y = ClampAxis(_pan.y, size.y, _mapSize.y);

            _content.style.left = _pan.x;
            _content.style.top = _pan.y;
        }

        // Map bigger than the screen: keep it covering the screen. Smaller: center it.
        private static float ClampAxis(float pan, float viewport, float map) =>
            map <= viewport ? (viewport - map) * 0.5f : Mathf.Clamp(pan, viewport - map, 0f);

        private Vector2 ViewportSize()
        {
            if (_viewport == null)
                return Vector2.zero;

            var size = _viewport.contentRect.size;

            if (float.IsNaN(size.x) || float.IsNaN(size.y) || size.x <= 0f || size.y <= 0f)
                return Vector2.zero;

            return size;
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.target is VisualElement target && target.GetFirstOfType<LevelMapMarker>() != null)
                return;

            _dragging = true;
            _dragPointer = evt.pointerId;
            _viewport.CapturePointer(evt.pointerId);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_dragging || evt.pointerId != _dragPointer)
                return;

            _pan += (Vector2)evt.deltaPosition;
            ApplyPan();
        }

        private void OnPointerUp(PointerUpEvent evt) => StopDrag(evt.pointerId);

        private void OnPointerCancel(PointerCancelEvent evt) => StopDrag(evt.pointerId);

        private void StopDrag(int pointerId)
        {
            if (!_dragging || pointerId != _dragPointer)
                return;

            _dragging = false;
            _dragPointer = -1;

            if (_viewport.HasPointerCapture(pointerId))
                _viewport.ReleasePointer(pointerId);
        }

        private void OnBackClicked() => BackRequested?.Invoke();

        private void OnDestroy()
        {
            if (_backButton != null)
                _backButton.clickable.clicked -= OnBackClicked;

            _clouds?.Dispose();
        }

        private sealed class Marker
        {
            public readonly LevelMapMarker View;
            public LevelNodeState State;
            public LevelNodeState SeenState;
            public string Title;
            public int Stars;
            public Vector2 Position;

            // Reveal animation progress, 1 when idle.
            public float HoleScale = 1f;
            public float Grow = 1f;

            public Marker(LevelMapMarker view) => View = view;
        }
    }
}
