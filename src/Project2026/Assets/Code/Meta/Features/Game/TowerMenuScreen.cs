using Code.Game.Common.UI;
using Code.Game.StaticData.Configs;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Code.Meta.Features.Game
{
    public class TowerMenuScreen : MonoBehaviour
    {
        [SerializeField] private EntityConfig _testTowerUpgrate;

        private GameScreen _gameScreen;
        private UIService _uIService;

        private VisualElement _towerBuildMenu;
        private VisualElement _towersContainer;

        private Button _closeBuildMenuButton;
        private Button _towerTestButton;

        private GameEntity _currentTowerEntity;

        [Inject]
        public void Construct(UIService uIService, GameScreen gameScreen)
        {
            _gameScreen = gameScreen;
            _uIService = uIService;
        }

        private void Start()
        {
            _towerBuildMenu = _gameScreen.GetVisualElement("TowerBuildMenu");
            _towersContainer = _gameScreen.GetVisualElement("TowersContainer");

            _closeBuildMenuButton = _gameScreen.GetButton("CloseButton");
            _towerTestButton = _gameScreen.GetButton("ButtonTestTower");

            _towerTestButton.clickable.clicked += CreateTestTower;
            _closeBuildMenuButton.clickable.clicked += Close;
        }

        public void Open(Vector2 screenPos, GameEntity entity)
        {
            _currentTowerEntity = entity;

            _uIService.MoveToScreenToPos(screenPos, _gameScreen.GetRoot(), _towerBuildMenu);
            _uIService.Show(_towerBuildMenu).AsAsyncUnitUniTask();

            _towersContainer.pickingMode = PickingMode.Position;
            _towerTestButton.pickingMode = PickingMode.Position;
            _closeBuildMenuButton.pickingMode = PickingMode.Position;
        }

        public void Close()
        {
            _currentTowerEntity = null;

            _closeBuildMenuButton.pickingMode = PickingMode.Ignore;
            _towersContainer.pickingMode = PickingMode.Ignore;
            _towerTestButton.pickingMode = PickingMode.Ignore;

            _uIService.Hide(_towerBuildMenu).AsAsyncUnitUniTask();
        }

        private void CreateTestTower()
        {
            if(_currentTowerEntity != null)
            {
                _currentTowerEntity.isTowerBuildRequest = true;
                _currentTowerEntity.AddEntityConfig(_testTowerUpgrate);
            }

            Close();
        }

        private void OnDestroy()
        {
            _closeBuildMenuButton.clickable.clicked -= Close;
            _towerTestButton.clickable.clicked -= CreateTestTower;
        }
    }
}