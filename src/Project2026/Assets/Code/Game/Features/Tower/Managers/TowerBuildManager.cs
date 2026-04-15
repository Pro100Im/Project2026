using Code.Game.Input.Service;
using Code.Game.StaticData.Configs;
using Code.Infrastructure.View;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace Code.Game.Features.Tower.Managers
{
    public class TowerBuildManager : MonoBehaviour
    {
        [SerializeField] private EntityConfig _testTowerUpgrate;
        [SerializeField] private EntityBehaviour _tower;
        [SerializeField] private SpriteRenderer _touchZone;

        private IInputService _inputService;
        private TowerUIManager _towerUIManager;

        [Inject]
        public void Construct(TowerUIManager towerUIManager, IInputService inputService)
        {
            _inputService = inputService;
            _towerUIManager = towerUIManager;
        }

        private void Start()
        {
            _inputService.SubscribeOnClick(OnClick);
        }

        private async void OnClick(InputAction.CallbackContext context)
        {
            if (!_tower.Entity.isTowerPlace)
                return;

            var point = _inputService.GetPointer();

            if (_towerUIManager.IsPointerOverUI(point))
            {
                Debug.Log("Pointer is over UI, ignoring click.");

                return;
            }     

            var worldPos = _inputService.GetWorldPointer();

            if (_touchZone.bounds.Contains(worldPos))
            {
                _towerUIManager.TowerTestButton.clickable.clicked -= CreateTestTower;
                _towerUIManager.CloseButton.clickable.clicked -= Cancel;

                await _towerUIManager.Open();

                _towerUIManager.TowerTestButton.clickable.clicked += CreateTestTower;
                _towerUIManager.CloseButton.clickable.clicked += Cancel;
            }
        }

        private async void CreateTestTower()
        {
            _towerUIManager.TowerTestButton.clickable.clicked -= CreateTestTower;
            _towerUIManager.CloseButton.clickable.clicked -= Cancel;

            await _towerUIManager.Close();

            if (!_tower.Entity.isTowerPlace)
                return;

            _tower.Entity.isTowerBuildRequest = true;
            _tower.Entity.AddEntityConfig(_testTowerUpgrate);
        }

        private void Cancel()
        {
            _towerUIManager.Close().AsAsyncUnitUniTask();

            _towerUIManager.TowerTestButton.clickable.clicked -= CreateTestTower;
            _towerUIManager.CloseButton.clickable.clicked -= Cancel;
        }

        private void OnDestroy()
        {
            _inputService.UnSubscribeOnClick(OnClick);
        }
    }
}