using Code.Game.Common.Entity;
using Code.Game.Common.UI;
using Code.Game.StaticData.Configs;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Code.Meta.Features.Game
{
    public class TowerMenu : MonoBehaviour
    {
        [SerializeField] private EntityConfig _archerTowerUpgrade;
        [SerializeField] private EntityConfig _iceTowerUpgrade;
        [SerializeField] private EntityConfig _fireTowerUpgrade;

        private GameScreen _gameScreen;
        private UIService _uIService;

        private VisualElement _towerBuildMenu;
        private VisualElement _towerBuildsContainer;

        private Button _archerTowerButton;
        private Button _iceTowerButton;
        private Button _fireTowerButton;
        private Button _towerBuildsCloseButton;

        private VisualElement _towerUpgradeMenu;
        private VisualElement _towerUpgradesContainer;

        private Button _towerUpgradesCloseButton;
        private Button _towerUpgrade1Button;
        private Button _towerUpgrade2Button;

        private Image _towerUpgradeIcon1;
        private Image _towerUpgradeIcon2;

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
            _towerBuildsContainer = _gameScreen.GetVisualElement("TowerBuildsContainer");

            _archerTowerButton = _gameScreen.GetButton("ArcherTower");
            _iceTowerButton = _gameScreen.GetButton("IceTower");
            _fireTowerButton = _gameScreen.GetButton("FireTower");
            _towerBuildsCloseButton = _gameScreen.GetButton("TowerBuildsCloseButton");

            _archerTowerButton.clickable.clicked += CreateArcherTower;
            _iceTowerButton.clickable.clicked += CreateIceTower;
            _fireTowerButton.clickable.clicked += CreateFireTower;
            _towerBuildsCloseButton.clickable.clicked += CloseRequest;

            _towerUpgradeMenu = _gameScreen.GetVisualElement("TowerUpgradeMenu");
            _towerUpgradesContainer = _gameScreen.GetVisualElement("TowerUpgradesContainer");

            _towerUpgrade1Button = _gameScreen.GetButton("TowerUpgrade1");
            _towerUpgrade2Button = _gameScreen.GetButton("TowerUpgrade2");
            _towerUpgradesCloseButton = _gameScreen.GetButton("TowerUpgradesCloseButton");

            _towerUpgradeIcon1 = _gameScreen.GetImage("UpgradeIcon1");
            _towerUpgradeIcon2 = _gameScreen.GetImage("UpgradeIcon2");

            _towerUpgrade1Button.clickable.clicked += UpgradeTower1;
            _towerUpgrade2Button.clickable.clicked += UpgradeTower2;
            _towerUpgradesCloseButton.clickable.clicked += CloseRequest;
        }

        public void OpenTowerBuildMenu(Vector2 screenPos, GameEntity entity)
        {
            _currentTowerEntity = entity;

            _uIService.MoveToScreenToPos(screenPos, _gameScreen.GetRoot(), _towerBuildMenu);
            _uIService.Show(_towerBuildMenu).AsAsyncUnitUniTask();

            _towerBuildsContainer.pickingMode = PickingMode.Position;

            _archerTowerButton.pickingMode = PickingMode.Position;
            _iceTowerButton.pickingMode = PickingMode.Position;
            _fireTowerButton.pickingMode = PickingMode.Position;
            _towerBuildsCloseButton.pickingMode = PickingMode.Position;
        }

        private void CloseRequest()
        {
            if(_currentTowerEntity != null)
            {
                var entityClick = CreateInputEntity.Empty();
                entityClick.AddTargetId(_currentTowerEntity.id.Value);
                entityClick.isInput = true;
            }
        }

        public void CloseTowerBuilds()
        {
            _currentTowerEntity = null;

            _towerBuildsContainer.pickingMode = PickingMode.Ignore;
            _archerTowerButton.pickingMode = PickingMode.Ignore;
            _iceTowerButton.pickingMode = PickingMode.Ignore;
            _fireTowerButton.pickingMode = PickingMode.Ignore;
            _towerBuildsCloseButton.pickingMode = PickingMode.Ignore;

            _uIService.Hide(_towerBuildMenu).AsAsyncUnitUniTask();
        }

        private void CreateArcherTower()
        {
            if (_currentTowerEntity != null)
            {
                _currentTowerEntity.isTowerBuildRequest = true;
                _currentTowerEntity.AddEntityConfig(_archerTowerUpgrade);
            }

            CloseRequest();
        }

        private void CreateIceTower()
        {
            if (_currentTowerEntity != null)
            {
                _currentTowerEntity.isTowerBuildRequest = true;
                _currentTowerEntity.AddEntityConfig(_iceTowerUpgrade);
            }

            CloseRequest();
        }

        private void CreateFireTower()
        {
            if (_currentTowerEntity != null)
            {
                _currentTowerEntity.isTowerBuildRequest = true;
                _currentTowerEntity.AddEntityConfig(_fireTowerUpgrade);
            }

            CloseRequest();
        }

        public void OpenTowerUpgradeMenu(Vector2 screenPos, GameEntity entity)
        {
            _currentTowerEntity = entity;

            if (entity.towerUpgrade.Value.Length > 1)
            {
                _towerUpgradeIcon1.sprite = entity.towerUpgradeIcon.Value[0];
                _uIService.Show(_towerUpgrade1Button).AsAsyncUnitUniTask();
                _towerUpgrade1Button.pickingMode = PickingMode.Position;

                _towerUpgradeIcon2.sprite = entity.towerUpgradeIcon.Value[1];
                _uIService.Show(_towerUpgrade2Button).AsAsyncUnitUniTask();
                _towerUpgrade2Button.pickingMode = PickingMode.Position;
            }
            else
            {
                _towerUpgrade2Button.pickingMode = PickingMode.Ignore;
                _uIService.Hide(_towerUpgrade2Button).AsAsyncUnitUniTask();
                _towerUpgradeIcon1.sprite = entity.towerUpgradeIcon.Value[0];
                _uIService.Show(_towerUpgrade1Button).AsAsyncUnitUniTask();
                _towerUpgrade1Button.pickingMode = PickingMode.Position;
            }

            _uIService.MoveToScreenToPos(screenPos, _gameScreen.GetRoot(), _towerUpgradeMenu);
            _uIService.Show(_towerUpgradeMenu).AsAsyncUnitUniTask();

            _towerUpgradesContainer.pickingMode = PickingMode.Position;
            _towerUpgradesCloseButton.pickingMode = PickingMode.Position;
        }

        public void CloseTowerUpgrades()
        {
            _currentTowerEntity = null;

            _towerUpgradesCloseButton.pickingMode = PickingMode.Ignore;

            _towerUpgradesContainer.pickingMode = PickingMode.Ignore;

            _towerUpgrade1Button.pickingMode = PickingMode.Ignore;
            _towerUpgrade2Button.pickingMode = PickingMode.Ignore;
            _towerUpgradesCloseButton.pickingMode = PickingMode.Ignore;

            _uIService.Hide(_towerUpgradeMenu).AsAsyncUnitUniTask();
            _uIService.Hide(_towerUpgrade1Button).AsAsyncUnitUniTask();
            _uIService.Hide(_towerUpgrade2Button).AsAsyncUnitUniTask();
        }

        private void UpgradeTower2()
        {
            if (_currentTowerEntity != null)
                _currentTowerEntity.AddTowerUpgradeRequest(1);

            CloseRequest();
        }

        private void UpgradeTower1()
        {
            if (_currentTowerEntity != null)
                _currentTowerEntity.AddTowerUpgradeRequest(0);

            CloseRequest();
        }

        private void OnDestroy()
        {
            _archerTowerButton.clickable.clicked -= CreateArcherTower;
            _iceTowerButton.clickable.clicked -= CreateIceTower;
            _fireTowerButton.clickable.clicked -= CreateFireTower;
            _towerBuildsCloseButton.clickable.clicked -= CloseRequest;

            _towerUpgrade1Button.clickable.clicked -= UpgradeTower1;
            _towerUpgrade2Button.clickable.clicked -= UpgradeTower2;
            _towerUpgradesCloseButton.clickable.clicked -= CloseRequest;
        }
    }
}