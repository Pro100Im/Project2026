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
        private readonly List<Vector4> _holes = new(LevelMapClouds.MaxHoles);

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

        public void SetNode(string id, string title, LevelNodeState state, int stars, Vector2 position)
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
            marker.Position = position;
            marker.View.Set(title, state, stars);
            marker.View.PlaceAt(position);
        }

        public void SetLinks(List<LevelMapLink> links)
        {
            _roads?.Set(links);
            _links.Clear();
            _links.AddRange(links);
            SortMarkers();
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
        private void UpdateClouds()
        {
            if (_clouds == null || _veil == null)
                return;

            _holes.Clear();

            foreach (var marker in _markers.Values)
            {
                if (marker.State == LevelNodeState.Hidden)
                    continue;

                var radius = marker.State == LevelNodeState.Locked ? _veil.PreviewRadius : _veil.RevealRadius;
                AddHole(marker.Position + new Vector2(0f, -HoleLift), radius);
            }

            var step = Mathf.Max(10f, _veil.RoadStep);

            for (var i = 0; i < _links.Count; i++)
            {
                var link = _links[i];
                var samples = Mathf.CeilToInt(Vector2.Distance(link.From, link.To) / step);

                for (var s = 1; s < samples; s++)
                    AddHole(LevelMapRoads.Point(link.From, link.To, (float)s / samples), _veil.RoadRadius);
            }

            _clouds.SetHoles(_holes);
        }

        private void AddHole(Vector2 center, float radius)
        {
            if (_holes.Count < LevelMapClouds.MaxHoles)
                _holes.Add(new Vector4(center.x, center.y, radius, 0f));
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

            var focus = target ?? fallback ?? _mapSize * 0.5f;
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
            public Vector2 Position;

            public Marker(LevelMapMarker view) => View = view;
        }
    }
}
