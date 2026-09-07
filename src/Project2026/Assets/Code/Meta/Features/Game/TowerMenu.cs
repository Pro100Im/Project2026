using Code.Game.Common.Entity;
using Code.Game.Common.UI;
using Code.Game.StaticData.Configs;
using Cysharp.Threading.Tasks;
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

        private Button _archerTowerButton;
        private Button _iceTowerButton;
        private Button _fireTowerButton;
        private Button _towerBuildsCloseButton;

        private VisualElement _towerUpgradeMenu;

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

            _archerTowerButton = _gameScreen.GetButton("ArcherTower");
            _iceTowerButton = _gameScreen.GetButton("IceTower");
            _fireTowerButton = _gameScreen.GetButton("FireTower");
            _towerBuildsCloseButton = _gameScreen.GetButton("TowerBuildsCloseButton");

            _archerTowerButton.clickable.clicked += CreateArcherTower;
            _iceTowerButton.clickable.clicked += CreateIceTower;
            _fireTowerButton.clickable.clicked += CreateFireTower;
            _towerBuildsCloseButton.clickable.clicked += CloseAndDeselectRequest;

            _towerUpgradeMenu = _gameScreen.GetVisualElement("TowerUpgradeMenu");

            _towerUpgrade1Button = _gameScreen.GetButton("TowerUpgrade1");
            _towerUpgrade2Button = _gameScreen.GetButton("TowerUpgrade2");
            _towerUpgradesCloseButton = _gameScreen.GetButton("TowerUpgradesCloseButton");

            _towerUpgradeIcon1 = _gameScreen.GetImage("UpgradeIcon1");
            _towerUpgradeIcon2 = _gameScreen.GetImage("UpgradeIcon2");

            _towerUpgrade1Button.clickable.clicked += UpgradeTower1;
            _towerUpgrade2Button.clickable.clicked += UpgradeTower2;
            _towerUpgradesCloseButton.clickable.clicked += CloseAndDeselectRequest;

            _uIService.Hide(_towerBuildMenu).Forget();
            _uIService.Hide(_towerUpgradeMenu).Forget();
        }

        public void OpenTowerBuildMenu(Vector2 screenPos, GameEntity entity)
        {
            _currentTowerEntity = entity;

            _uIService.MoveToScreenToPos(screenPos, _gameScreen.GetRoot(), _towerBuildMenu);
            _uIService.Show(_towerBuildMenu).AsAsyncUnitUniTask();
        }

        private void CloseRequest()
        {
            var request = CreateMetaEntity.Empty();
            request.isTowerMenuCloseRequest = true;

            _currentTowerEntity = null;
        }

        private void CloseAndDeselectRequest()
        {
            var request = CreateMetaEntity.Empty();
            request.isTowerMenuCloseRequest = true;

            if (_currentTowerEntity != null && _currentTowerEntity.hasId)
                request.AddTargetId(_currentTowerEntity.id.Value);

            _currentTowerEntity = null;
        }

        public void CloseTowerBuilds()
        {
            _currentTowerEntity = null;

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

                _towerUpgradeIcon2.sprite = entity.towerUpgradeIcon.Value[1];
                _uIService.Show(_towerUpgrade2Button).AsAsyncUnitUniTask();
            }
            else
            {
                _uIService.Hide(_towerUpgrade2Button).AsAsyncUnitUniTask();
                _towerUpgradeIcon1.sprite = entity.towerUpgradeIcon.Value[0];
                _uIService.Show(_towerUpgrade1Button).AsAsyncUnitUniTask();
            }

            _uIService.MoveToScreenToPos(screenPos, _gameScreen.GetRoot(), _towerUpgradeMenu);
            _uIService.Show(_towerUpgradeMenu).AsAsyncUnitUniTask();
        }

        public void CloseTowerUpgrades()
        {
            _currentTowerEntity = null;

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
            if (_archerTowerButton != null)
                _archerTowerButton.clickable.clicked -= CreateArcherTower;
            if (_iceTowerButton != null)
                _iceTowerButton.clickable.clicked -= CreateIceTower;
            if (_fireTowerButton != null)
                _fireTowerButton.clickable.clicked -= CreateFireTower;
            if (_towerBuildsCloseButton != null)
                _towerBuildsCloseButton.clickable.clicked -= CloseAndDeselectRequest;

            if (_towerUpgrade1Button != null)
                _towerUpgrade1Button.clickable.clicked -= UpgradeTower1;
            if (_towerUpgrade2Button != null)
                _towerUpgrade2Button.clickable.clicked -= UpgradeTower2;
            if (_towerUpgradesCloseButton != null)
                _towerUpgradesCloseButton.clickable.clicked -= CloseAndDeselectRequest;
        }
    }
}
