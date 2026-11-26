using Code.Game.Common.Entity;
using Code.Game.Common.UI;
using Code.Game.StaticData.Configs;
using Code.Meta.UI.Common;
using Code.Meta.UI.MainMenu;
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

    /// <summary>
    /// Research tree view, built like the world map: a pannable content with a parchment backdrop,
    /// dashed roads, round node buttons and the cloud veil over what is not reachable yet.
    /// Draws what TownResearchMenuRenderSystem tells it and sends requests to the input context.
    /// </summary>
    public class TownResearchMenu : MonoBehaviour
    {
        private const float NodeSize = 76f;
        private const float CenterSize = 64f;

        // Free space around the outermost nodes, so the tree can be panned and the clouds have room.
        private const float ContentMargin = 420f;
        private const float RoadBend = 0.12f;

        private const string OwnedClass = "research-node--owned";
        private const string AvailableClass = "research-node--available";
        private const string LockedClass = "research-node--locked";
        private const string UnaffordableClass = "research-node--unaffordable";

        [SerializeField] private TownScreen _townScreen;

        private UIService _uIService;

        private VisualElement _menu;
        private VisualElement _tree;
        private VisualElement _content;
        private LevelMapRoads _roads;
        private CloudVeil _clouds;
        private CloudVeilSettings _veil;
        private LevelMapRevealPlayer _player;
        private VisualElement _center;
        private Button _closeButton;
        private VisualElement _detail;
        private Image _detailIcon;
        private Label _detailTitle;
        private Label _detailDescription;
        private Label _detailPrice;
        private Button _detailClose;
        private Button _detailBuy;
        private Label _detailBuyLabel;

        private ResearchId _selectedId;

        private readonly Dictionary<ResearchId, NodeView> _nodes = new(32);
        private readonly List<ResearchLink> _links = new(32);
        private readonly List<LevelMapLink> _roadLinks = new(32);
        private readonly List<float> _linkReveal = new(32);
        private readonly List<Vector4> _holes = new(CloudVeil.MaxHoles);

        private Vector2 _contentSize;
        private Vector2 _origin;
        private Vector2 _pan;
        private bool _centerPending;
        private bool _shown;
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
            _detailBuyLabel = _townScreen.GetLabel("TownResearchDetailBuyLabel");

            if (_menu == null || _tree == null || _closeButton == null || _detail == null || _detailClose == null || _detailBuy == null)
                return;

            BuildTree();

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
            _centerPending = true;
            _shown = false;
            _menu.style.display = DisplayStyle.Flex;
            _clouds.SetRunning(true);
            _uIService.Show(_menu).Forget();
            ApplyPan();
        }

        public void Hide()
        {
            if (_menu == null)
                return;

            StopDrag(_dragPointer);
            _player.Complete();
            _clouds.SetRunning(false);
            HideDetail();
            HideAsync(++_visibilityVersion).Forget();
        }

        public void SetVeil(CloudVeilSettings veil)
        {
            if (_clouds == null || _veil == veil)
                return;

            _veil = veil;
            _clouds.SetSettings(veil);
        }

        public void SetNode(ResearchId id, string title, string description, Sprite icon, int cost, ResearchNodeState state, Vector2 position, bool veiled)
        {
            if (_content == null)
                return;

            var node = GetOrCreate(id);

            // Uncovered while the player is looking at the tree: the reveal is animated in SetLinks.
            node.JustRevealed = _shown && node.Known && node.Veiled && !veiled;
            node.Known = true;
            node.Offset = position;
            node.Title = title;
            node.Description = description;
            node.Cost = cost;
            node.Price.text = cost.ToString();
            node.State = state;
            node.Veiled = veiled;
            node.HoleScale = 1f;
            node.Icon.sprite = icon;
            node.Icon.style.display = icon == null ? DisplayStyle.None : DisplayStyle.Flex;
            node.Button.style.display = veiled ? DisplayStyle.None : DisplayStyle.Flex;
            node.Button.pickingMode = veiled ? PickingMode.Ignore : PickingMode.Position;
            ClearAppear(node);

            node.Button.EnableInClassList(OwnedClass, state == ResearchNodeState.Owned);
            node.Button.EnableInClassList(AvailableClass, state == ResearchNodeState.Available);
            node.Button.EnableInClassList(LockedClass, state == ResearchNodeState.Locked);
            node.Button.EnableInClassList(UnaffordableClass, state == ResearchNodeState.Unaffordable);

            if (id != _selectedId)
                return;

            if (veiled)
                HideDetail();
            else
                ShowDetail(node);
        }

        /// <summary>Called last in a render pass: lays the tree out and plays the reveal of new nodes.</summary>
        public void SetLinks(List<ResearchLink> links)
        {
            if (_content == null)
                return;

            _player.Complete();
            _links.Clear();
            _links.AddRange(links);

            Layout();
            PlayReveal();
            UpdateClouds();
            _shown = true;
        }

        private void BuildTree()
        {
            _tree.Clear();
            _tree.pickingMode = PickingMode.Position;

            _content = new VisualElement { pickingMode = PickingMode.Ignore };
            _content.AddToClassList("research-content");
            _tree.Add(_content);

            var backdrop = new LevelMapBackdrop();
            backdrop.SetSprite(null);
            _content.Add(backdrop);

            _roads = new LevelMapRoads { Bend = RoadBend, LineWidth = 5f };
            _content.Add(_roads);

            _center = new VisualElement { pickingMode = PickingMode.Ignore };
            _center.AddToClassList("research-center");
            var centerIcon = new VisualElement { pickingMode = PickingMode.Ignore };
            centerIcon.AddToClassList("research-center__icon");
            centerIcon.AddToClassList("menu-icon-house");
            _center.Add(centerIcon);
            _content.Add(_center);

            // On top of everything; nodes are inserted below it.
            _clouds = new CloudVeil();
            _content.Add(_clouds);

            _player = new LevelMapRevealPlayer(_content);

            _tree.RegisterCallback<GeometryChangedEvent>(_ => ApplyPan());
            _tree.RegisterCallback<PointerDownEvent>(OnPointerDown);
            _tree.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _tree.RegisterCallback<PointerUpEvent>(OnPointerUp);
            _tree.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
        }

        private NodeView GetOrCreate(ResearchId id)
        {
            if (_nodes.TryGetValue(id, out var existing))
                return existing;

            var button = new Button { pickingMode = PickingMode.Position };
            button.AddToClassList("research-node");

            var icon = new Image { pickingMode = PickingMode.Ignore };
            icon.AddToClassList("research-node__icon");
            button.Add(icon);

            // Shown by USS on owned nodes only.
            var star = new LevelMapStar();
            star.AddToClassList("research-node__star");
            button.Add(star);

            // Shown by USS while the node can be bought (or is too expensive).
            var price = new Label { pickingMode = PickingMode.Ignore };
            price.AddToClassList("research-node__price");
            button.Add(price);

            button.clickable.clicked += () => OpenDetail(id);
            _content.Insert(_content.IndexOf(_clouds), button);

            var node = new NodeView(button, icon, price);
            _nodes.Add(id, node);
            return node;
        }

        // Content covers every node (veiled ones too) plus a margin; the town sits at _origin.
        private void Layout()
        {
            var min = Vector2.zero;
            var max = Vector2.zero;

            foreach (var node in _nodes.Values)
            {
                min = Vector2.Min(min, node.Offset);
                max = Vector2.Max(max, node.Offset);
            }

            var margin = Vector2.one * ContentMargin;
            var size = max - min + margin * 2f;
            var origin = margin - min;

            if (size != _contentSize || origin != _origin)
            {
                // Keep the same spot of the tree under the same spot of the screen.
                _pan += _origin - origin;
                _contentSize = size;
                _origin = origin;
                _content.style.width = size.x;
                _content.style.height = size.y;
                _clouds.SetMapSize(size);
            }

            foreach (var node in _nodes.Values)
            {
                var point = _origin + node.Offset;
                node.Button.style.left = point.x - NodeSize * 0.5f;
                node.Button.style.top = point.y - NodeSize * 0.5f;
            }

            _center.style.left = _origin.x - CenterSize * 0.5f;
            _center.style.top = _origin.y - CenterSize * 0.5f;

            _roadLinks.Clear();
            _linkReveal.Clear();

            for (var i = 0; i < _links.Count; i++)
            {
                var link = _links[i];
                _roadLinks.Add(new LevelMapLink(_origin + link.From, _origin + link.To, link.Owned));
                _linkReveal.Add(1f);
            }

            _roads.Set(_roadLinks);
            ApplyPan();
        }

        // Clear holes: the town, every uncovered node, and along the roads between them.
        private void UpdateClouds(bool animated = false)
        {
            if (_veil == null)
                return;

            _holes.Clear();
            AddHole(_origin, _veil.RevealRadius, 1f);

            foreach (var node in _nodes.Values)
            {
                if (!node.Veiled)
                    AddHole(_origin + node.Offset, _veil.RevealRadius, node.HoleScale);
            }

            var step = Mathf.Max(10f, _veil.RoadStep);

            for (var i = 0; i < _roadLinks.Count; i++)
            {
                var link = _roadLinks[i];
                var reveal = _linkReveal[i];
                var samples = Mathf.CeilToInt(Vector2.Distance(link.From, link.To) / step);

                for (var s = 1; s < samples; s++)
                {
                    var along = (float)s / samples;
                    var scale = reveal >= 1f ? 1f : Mathf.Clamp01((reveal - along + 0.15f) / 0.3f);
                    AddHole(LevelMapRoads.Point(link.From, link.To, along, RoadBend), _veil.RoadRadius, scale);
                }
            }

            _clouds.SetHoles(_holes, animated);
        }

        private void AddHole(Vector2 center, float radius, float scale)
        {
            if (_holes.Count < CloudVeil.MaxHoles)
                _holes.Add(new Vector4(center.x, center.y, radius, scale));
        }

        // Nodes uncovered by a purchase: the road draws in, the clouds part, the node pops.
        private void PlayReveal()
        {
            var stagger = 0f;

            foreach (var node in _nodes.Values)
            {
                if (!node.JustRevealed)
                    continue;

                node.JustRevealed = false;

                var revealed = node;
                var at = stagger;
                var link = LinkTo(_origin + node.Offset);
                stagger += 0.12f;

                if (link >= 0)
                {
                    _player.Add(at, 0.45f, p =>
                    {
                        var eased = LevelMapRevealPlayer.EaseInOut(p);
                        _linkReveal[link] = eased;
                        _roads.SetProgress(link, eased);
                        UpdateClouds(true);
                    });
                }

                revealed.Button.pickingMode = PickingMode.Ignore;

                _player.Add(at + 0.2f, 0.6f, p =>
                {
                    revealed.HoleScale = LevelMapRevealPlayer.EaseOut(p);
                    UpdateClouds(true);
                });

                _player.Add(at + 0.4f, 0.35f,
                    p =>
                    {
                        var scale = Mathf.LerpUnclamped(0.5f, 1f, LevelMapRevealPlayer.BackOut(p));
                        revealed.Button.style.opacity = Mathf.Clamp01(p * 2f);
                        revealed.Button.style.scale = new Scale(new Vector2(scale, scale));
                    },
                    () =>
                    {
                        ClearAppear(revealed);
                        revealed.Button.pickingMode = PickingMode.Position;
                    });
            }

            _player.Play();
        }

        // Hands scale and opacity back to USS (hover scale, locked opacity).
        private static void ClearAppear(NodeView node)
        {
            node.Button.style.opacity = StyleKeyword.Null;
            node.Button.style.scale = StyleKeyword.Null;
        }

        private int LinkTo(Vector2 point)
        {
            for (var i = 0; i < _roadLinks.Count; i++)
            {
                if (_roadLinks[i].To == point)
                    return i;
            }

            return -1;
        }

        private void ApplyPan()
        {
            if (_content == null || _contentSize == Vector2.zero)
                return;

            var size = _tree.contentRect.size;

            if (float.IsNaN(size.x) || float.IsNaN(size.y) || size.x < 1f || size.y < 1f)
                return;

            if (_centerPending)
            {
                _pan = size * 0.5f - _origin;
                _centerPending = false;
            }

            _pan.x = ClampAxis(_pan.x, size.x, _contentSize.x);
            _pan.y = ClampAxis(_pan.y, size.y, _contentSize.y);

            _content.style.left = _pan.x;
            _content.style.top = _pan.y;
        }

        // Content bigger than the window: keep it covering the window. Smaller: center it.
        private static float ClampAxis(float pan, float viewport, float content) =>
            content <= viewport ? (viewport - content) * 0.5f : Mathf.Clamp(pan, viewport - content, 0f);

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

            if (_tree.HasPointerCapture(pointerId))
                _tree.ReleasePointer(pointerId);
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

            if (_detailBuyLabel != null)
                _detailBuyLabel.text = owned ? "Owned" : "Buy";

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

            _clouds?.Dispose();
        }

        private sealed class NodeView
        {
            public readonly Button Button;
            public readonly Image Icon;
            public readonly Label Price;
            public string Title;
            public string Description;
            public int Cost;
            public ResearchNodeState State;
            public Vector2 Offset;
            public bool Veiled;

            // Reveal animation state.
            public bool Known;
            public bool JustRevealed;
            public float HoleScale = 1f;

            public NodeView(Button button, Image icon, Label price)
            {
                Button = button;
                Icon = icon;
                Price = price;
            }
        }
    }
}
