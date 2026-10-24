using Code.Game.Common.Entity;
using Code.Game.Common.UI;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Code.Meta.Features.Town
{
    public readonly struct ResearchLink
    {
        public readonly Vector2 From;
        public readonly Vector2 To;
        public readonly bool Owned;

        public ResearchLink(Vector2 from, Vector2 to, bool owned)
        {
            From = from;
            To = to;
            Owned = owned;
        }
    }

    public class TownResearchMenu : MonoBehaviour
    {
        private const float PreferredNodeSize = 76f;

        private const string OwnedClass = "research-node--owned";
        private const string AvailableClass = "research-node--available";
        private const string LockedClass = "research-node--locked";
        private const string UnaffordableClass = "research-node--unaffordable";

        [SerializeField] private TownScreen _townScreen;

        private UIService _uIService;

        private VisualElement _menu;
        private VisualElement _tree;
        private ResearchTreeGraph _graph;
        private ResearchFog _fog;
        private VisualElement _center;
        private Button _closeButton;
        private VisualElement _detail;
        private Image _detailIcon;
        private Label _detailTitle;
        private Label _detailDescription;
        private Label _detailPrice;
        private Button _detailClose;
        private Button _detailBuy;

        private ResearchId _selectedId;

        private readonly Dictionary<ResearchId, NodeView> _nodes = new(32);
        private readonly List<ResearchLink> _links = new(32);
        private readonly List<Vector2> _fogPoints = new(32);

        private Vector2 _pan;
        private bool _dragging;
        private int _dragPointer = -1;
        private int _visibilityVersion;

        [Inject]
        public void Construct(UIService uIService)
        {
            _uIService = uIService;
        }

        private void Start()
        {
            _menu = _townScreen.GetVisualElement("TownResearchMenu");
            _tree = _townScreen.GetVisualElement("TownResearchTree");
            _closeButton = _townScreen.GetButton("TownResearchCloseButton");
            _detail = _townScreen.GetVisualElement("TownResearchDetail");
            _detailIcon = _townScreen.GetImage("TownResearchDetailIcon");
            _detailTitle = _townScreen.GetLabel("TownResearchDetailTitle");
            _detailDescription = _townScreen.GetLabel("TownResearchDetailDescription");
            _detailPrice = _townScreen.GetLabel("TownResearchDetailPrice");
            _detailClose = _townScreen.GetButton("TownResearchDetailClose");
            _detailBuy = _townScreen.GetButton("TownResearchDetailBuy");

            if (_menu == null || _tree == null || _closeButton == null || _detail == null || _detailClose == null || _detailBuy == null)
                return;

            _graph = new ResearchTreeGraph();
            _graph.pickingMode = PickingMode.Ignore;
            _graph.style.position = Position.Absolute;
            _graph.style.left = 0f;
            _graph.style.right = 0f;
            _graph.style.top = 0f;
            _graph.style.bottom = 0f;
            _tree.Insert(0, _graph);

            _fog = new ResearchFog();
            _tree.Insert(1, _fog);

            _center = new VisualElement();
            _center.AddToClassList("research-center");
            _center.pickingMode = PickingMode.Ignore;
            var centerLabel = new Label("Town") { pickingMode = PickingMode.Ignore };
            centerLabel.AddToClassList("research-center__label");
            _center.Add(centerLabel);
            _tree.Add(_center);

            _tree.pickingMode = PickingMode.Position;
            _tree.RegisterCallback<GeometryChangedEvent>(_ => ApplyLayout());
            _tree.RegisterCallback<PointerDownEvent>(OnPointerDown);
            _tree.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _tree.RegisterCallback<PointerUpEvent>(OnPointerUp);
            _tree.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
            _closeButton.clickable.clicked += CloseRequest;
            _detailClose.clickable.clicked += HideDetail;
            _detailBuy.clickable.clicked += BuySelected;

            _menu.style.display = DisplayStyle.None;
            _uIService.Hide(_menu).Forget();
        }

        public void Show()
        {
            if (_menu == null)
                return;

            _visibilityVersion++;
            _pan = Vector2.zero;
            _menu.style.display = DisplayStyle.Flex;
            _uIService.Show(_menu).Forget();
            ApplyLayout();
        }

        public void Hide()
        {
            if (_menu == null)
                return;

            HideDetail();
            HideAsync(++_visibilityVersion).Forget();
        }

        public void SetNode(ResearchId id, string title, string description, Sprite icon, int cost, ResearchNodeState state, Vector2 position, bool veiled)
        {
            if (_tree == null)
                return;

            var node = GetOrCreate(id);
            node.Offset = position;
            node.Title = title;
            node.Description = description;
            node.Cost = cost;
            node.State = state;
            node.Veiled = veiled;
            node.Icon.sprite = icon;
            node.Icon.style.display = veiled || icon == null ? DisplayStyle.None : DisplayStyle.Flex;
            node.Button.style.display = veiled ? DisplayStyle.None : DisplayStyle.Flex;
            node.Button.pickingMode = veiled ? PickingMode.Ignore : PickingMode.Position;

            node.Available = state == ResearchNodeState.Available;
            node.Unaffordable = state == ResearchNodeState.Unaffordable;
            node.Button.EnableInClassList(OwnedClass, state == ResearchNodeState.Owned);
            node.Button.EnableInClassList(AvailableClass, state == ResearchNodeState.Available);
            node.Button.EnableInClassList(LockedClass, state == ResearchNodeState.Locked);
            node.Button.EnableInClassList(UnaffordableClass, state == ResearchNodeState.Unaffordable);
            ApplyLayout();

            if (id != _selectedId)
                return;

            if (veiled)
                HideDetail();
            else
                ShowDetail(node);
        }

        public void SetLinks(List<ResearchLink> links)
        {
            _links.Clear();

            for (var i = 0; i < links.Count; i++)
                _links.Add(links[i]);

            ApplyLayout();
        }

        private NodeView GetOrCreate(ResearchId id)
        {
            if (_nodes.TryGetValue(id, out var existing))
                return existing;

            var button = new Button();
            button.pickingMode = PickingMode.Position;
            button.AddToClassList("research-node");

            var icon = new Image { pickingMode = PickingMode.Ignore };
            icon.AddToClassList("research-node__icon");

            button.Add(icon);
            button.clickable.clicked += () => OpenDetail(id);

            _tree.Add(button);

            var node = new NodeView(button, icon);
            _nodes.Add(id, node);
            return node;
        }

        private void ApplyLayout()
        {
            if (_tree == null)
                return;

            var width = _tree.resolvedStyle.width;
            var height = _tree.resolvedStyle.height;

            if (float.IsNaN(width) || float.IsNaN(height) || width < 1f || height < 1f)
                return;

            var center = new Vector2(width * 0.5f, height * 0.5f) + _pan;
            var half = PreferredNodeSize * 0.5f;
            var centerSize = PreferredNodeSize * 0.72f;

            foreach (var pair in _nodes)
            {
                var button = pair.Value.Button;
                var placed = pair.Value.Offset;

                SetNodeSize(button, PreferredNodeSize);
                button.style.left = center.x + placed.x - half;
                button.style.top = center.y + placed.y - half;
            }

            _fogPoints.Clear();
            _fogPoints.Add(center);

            foreach (var pair in _nodes)
            {
                if (pair.Value.Veiled)
                    continue;

                pair.Value.Button.BringToFront();
                _fogPoints.Add(center + pair.Value.Offset);
            }

            SetNodeSize(_center, centerSize);
            _center.style.left = center.x - centerSize * 0.5f;
            _center.style.top = center.y - centerSize * 0.5f;
            _center.BringToFront();
            _graph.Set(center, _links);
            _fog.Redraw(width, height, _fogPoints);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.target is Button)
                return;

            _dragging = true;
            _dragPointer = evt.pointerId;
            _tree.CapturePointer(evt.pointerId);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_dragging || evt.pointerId != _dragPointer)
                return;

            _pan += (Vector2)evt.deltaPosition;
            ApplyLayout();
        }

        private void OnPointerUp(PointerUpEvent evt) => StopDrag(evt.pointerId);

        private void OnPointerCancel(PointerCancelEvent evt) => StopDrag(evt.pointerId);

        private void StopDrag(int pointerId)
        {
            if (!_dragging || pointerId != _dragPointer)
                return;

            _dragging = false;
            _dragPointer = -1;

            if (_tree.HasPointerCapture(pointerId))
                _tree.ReleasePointer(pointerId);
        }

        private static void SetNodeSize(VisualElement element, float size)
        {
            element.style.width = size;
            element.style.height = size;
            element.style.minWidth = size;
            element.style.minHeight = size;
            element.style.maxWidth = size;
            element.style.maxHeight = size;
            element.style.borderTopLeftRadius = 0f;
            element.style.borderTopRightRadius = 0f;
            element.style.borderBottomLeftRadius = 0f;
            element.style.borderBottomRightRadius = 0f;
        }

        private void OpenDetail(ResearchId id)
        {
            if (!_nodes.TryGetValue(id, out var node) || node.Veiled)
                return;

            _selectedId = id;
            ShowDetail(node);
        }

        private void ShowDetail(NodeView node)
        {
            if (_detail == null)
                return;

            var owned = node.State == ResearchNodeState.Owned;

            _detailTitle.text = node.Title;
            _detailDescription.text = node.Description;
            _detailIcon.sprite = node.Icon.sprite;
            _detailIcon.style.display = node.Icon.sprite == null ? DisplayStyle.None : DisplayStyle.Flex;
            _detailPrice.text = owned ? "Owned" : node.Cost.ToString();
            _detailBuy.text = owned ? "Owned" : $"Buy {node.Cost}";
            _detailBuy.SetEnabled(node.State == ResearchNodeState.Available);
            _detail.style.display = DisplayStyle.Flex;
            _detail.BringToFront();
        }

        private void HideDetail()
        {
            _selectedId = ResearchId.None;

            if (_detail != null)
                _detail.style.display = DisplayStyle.None;
        }

        private void BuySelected()
        {
            if (_selectedId == ResearchId.None || !_nodes.TryGetValue(_selectedId, out var node))
                return;

            if (node.State != ResearchNodeState.Available)
                return;

            Purchase(_selectedId);
        }

        private static void Purchase(ResearchId id)
        {
            var request = CreateInputEntity.Empty();
            request.AddTownResearchPurchaseRequested(id);
            request.isInput = true;
        }

        private void CloseRequest()
        {
            var request = CreateInputEntity.Empty();
            request.isTownResearchMenuCloseRequested = true;
            request.isInput = true;
        }

        private async UniTaskVoid HideAsync(int version)
        {
            await _uIService.Hide(_menu);

            if (version == _visibilityVersion)
                _menu.style.display = DisplayStyle.None;
        }

        private void OnDestroy()
        {
            if (_closeButton != null)
                _closeButton.clickable.clicked -= CloseRequest;

            if (_detailClose != null)
                _detailClose.clickable.clicked -= HideDetail;

            if (_detailBuy != null)
                _detailBuy.clickable.clicked -= BuySelected;

            _fog?.Dispose();
        }

        private sealed class NodeView
        {
            public readonly Button Button;
            public readonly Image Icon;
            public string Title;
            public string Description;
            public int Cost;
            public ResearchNodeState State;
            public Vector2 Offset;
            public bool Available;
            public bool Unaffordable;
            public bool Veiled;

            public NodeView(Button button, Image icon)
            {
                Button = button;
                Icon = icon;
            }
        }
    }

    public class ResearchTreeGraph : VisualElement
    {
        private Vector2 _center;
        private readonly List<ResearchLink> _links = new(32);

        public ResearchTreeGraph()
        {
            generateVisualContent += OnGenerateVisualContent;
        }

        public void Set(Vector2 center, List<ResearchLink> links)
        {
            _center = center;
            _links.Clear();

            for (var i = 0; i < links.Count; i++)
                _links.Add(links[i]);

            MarkDirtyRepaint();
        }

        private void OnGenerateVisualContent(MeshGenerationContext context)
        {
            var painter = context.painter2D;
            painter.lineWidth = 4f;
            painter.lineCap = LineCap.Round;

            for (var i = 0; i < _links.Count; i++)
            {
                var link = _links[i];
                painter.strokeColor = link.Owned
                    ? new Color(0.96f, 0.78f, 0.35f, 0.95f)
                    : new Color(0.45f, 0.34f, 0.22f, 0.7f);
                painter.BeginPath();
                painter.MoveTo(_center + link.From);
                painter.LineTo(_center + link.To);
                painter.Stroke();
            }
        }
    }

    public sealed class ResearchFog : VisualElement
    {
        public const int MaxCircles = 32;
        public const float Radius = 100f;
        public const float Feather = 36f;

        private readonly Vector4[] _circles = new Vector4[MaxCircles];

        private RenderTexture _texture;
        private Material _material;

        public ResearchFog()
        {
            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;
            style.left = 0f;
            style.right = 0f;
            style.top = 0f;
            style.bottom = 0f;
            style.backgroundSize = new BackgroundSize(Length.Percent(100f), Length.Percent(100f));

            var shader = Shader.Find("Project2026/ResearchFog");

            if (shader != null)
                _material = new Material(shader);
        }

        public void Redraw(float width, float height, List<Vector2> points)
        {
            if (_material == null)
                return;

            var pixelWidth = Mathf.Max(1, Mathf.RoundToInt(width));
            var pixelHeight = Mathf.Max(1, Mathf.RoundToInt(height));
            EnsureTexture(pixelWidth, pixelHeight);

            var count = points.Count;

            if (count > MaxCircles)
                count = MaxCircles;

            for (var i = 0; i < count; i++)
                _circles[i] = new Vector4(points[i].x, points[i].y, Radius, Feather);

            _material.SetVectorArray("_Circles", _circles);
            _material.SetFloat("_CircleCount", count);
            _material.SetVector("_TargetSize", new Vector4(pixelWidth, pixelHeight, 0f, 0f));
            Graphics.Blit(Texture2D.whiteTexture, _texture, _material);
            style.backgroundImage = Background.FromRenderTexture(_texture);
        }

        public void Dispose()
        {
            style.backgroundImage = new StyleBackground();

            if (_texture != null)
            {
                _texture.Release();
                UnityEngine.Object.Destroy(_texture);
                _texture = null;
            }

            if (_material != null)
            {
                UnityEngine.Object.Destroy(_material);
                _material = null;
            }
        }

        private void EnsureTexture(int width, int height)
        {
            if (_texture != null && _texture.width == width && _texture.height == height)
                return;

            if (_texture != null)
            {
                _texture.Release();
                UnityEngine.Object.Destroy(_texture);
            }

            _texture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            _texture.Create();
        }
    }
}
