using Code.Game.Common.UI;
using Code.Game.StaticData.Configs;
using Code.Infrastructure.View;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Code.Game.Features.Tower.Managers
{
    public class TowerBuildManager : MonoBehaviour
    {
        [SerializeField] private EntityConfig _testTowerUpgrate;
        [SerializeField] private EntityBehaviour _tower;
        [SerializeField] private UIDocument _doc;

        private UIService _uIService;

        private VisualElement _towerBuilds;
        private VisualElement _container;

        private Button _openButton;
        private Button _closeButton;
        private Button _towerTestButton;

        [Inject]
        private void Construct(UIService uIService)
        {
            _uIService = uIService;
        }

        private void Start()
        {
            var root = _doc.rootVisualElement;

            _towerBuilds = root.Q<VisualElement>("TowerBuilds");
            _container = root.Q<VisualElement>("Container");

            _openButton = root.Q<Button>("OpenButton");
            _closeButton = root.Q<Button>("CloseButton");
            _towerTestButton = root.Q<Button>("ButtonTestTower");

            _openButton.clickable.clicked += Open;
            _closeButton.clickable.clicked += Close;
            _towerTestButton.clickable.clicked += CreateTestTower;
        }

        private async void Open()
        {
            _openButton.pickingMode = PickingMode.Ignore;

            await _uIService.Show(_towerBuilds);

            _container.pickingMode = PickingMode.Position;
            _closeButton.pickingMode = PickingMode.Position;
        }

        private async void Close()
        {
            _closeButton.pickingMode = PickingMode.Ignore;
            _container.pickingMode = PickingMode.Ignore;

            await _uIService.Hide(_towerBuilds);

            _openButton.pickingMode = PickingMode.Position;
        }

        private void CreateTestTower()
        {
            _closeButton.pickingMode = PickingMode.Ignore;
            _container.pickingMode = PickingMode.Ignore;

            _tower.Entity.isTowerBuildRequest = true;
            _tower.Entity.AddEntityConfig(_testTowerUpgrate);

            _uIService.Hide(_towerBuilds).AsTask();
        }

        private void OnDestroy()
        {
            _openButton.clickable.clicked -= Open;
            _closeButton.clickable.clicked -= Close;
            _towerTestButton.clickable.clicked -= CreateTestTower;
        }
    }
}