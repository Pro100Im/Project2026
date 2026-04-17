using Code.Game.Common.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Code.Game.Features.Tower.Managers
{
    public class TowerUIManager : MonoBehaviour
    {
        [SerializeField] private UIDocument _doc;

        private UIService _uIService;

        private VisualElement _towerBuilds;
        private VisualElement _container;

        public Button CloseButton { get; private set; }
        public Button TowerTestButton { get; private set; }

        [Inject]
        public void Construct(UIService uIService)
        {
            _uIService = uIService;
        }

        private void Start()
        {
            var root = _doc.rootVisualElement;

            _towerBuilds = root.Q<VisualElement>("TowersBuildMenu");
            _container = root.Q<VisualElement>("Container");

            CloseButton = root.Q<Button>("CloseButton");
            TowerTestButton = root.Q<Button>("ButtonTestTower");
        }

        public async UniTask Open(Vector2 screenPoint)
        {
            var root = _doc.rootVisualElement;

            var localPos = new Vector2(screenPoint.x, Screen.height - screenPoint.y);
            var clampedX = Mathf.Clamp(localPos.x, 0, root.resolvedStyle.width - _towerBuilds.resolvedStyle.width);
            var clampedY = Mathf.Clamp(localPos.y, 0, root.resolvedStyle.height - _towerBuilds.resolvedStyle.height);

            _towerBuilds.style.left = clampedX;
            _towerBuilds.style.top = clampedY;

            await _uIService.Show(_towerBuilds);

            _container.pickingMode = PickingMode.Position;
            CloseButton.pickingMode = PickingMode.Position;
        }

        public async UniTask Close()
        {
            CloseButton.pickingMode = PickingMode.Ignore;
            _container.pickingMode = PickingMode.Ignore;

            await _uIService.Hide(_towerBuilds).AsAsyncUnitUniTask();
        }

        public bool IsPointerOverUI(Vector2 screenPos)
        {
            return _uIService.IsPointerOverUI(screenPos, _towerBuilds);
        }
    }
}