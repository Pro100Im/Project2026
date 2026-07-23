using Code.Game.Common.Entity;
using Code.Game.Common.UI;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Code.Meta.Features.Game
{
    public class GameMenu : MonoBehaviour
    {
        private GameScreen _gameScreen;
        private UIService _uIService;

        private VisualElement _gameMenu;
        private Image _mask;

        private Button _menuButton;
        private Button _cancelButton;

        [Inject]
        public void Construct(UIService uIService, GameScreen gameScreen)
        {
            _gameScreen = gameScreen;
            _uIService = uIService;
        }

        private void Start()
        {
            //_gameMenu = _gameScreen.GetVisualElement("GameMenu");
            //_mask = _gameScreen.GetImage("Mask");

            _menuButton = _gameScreen.GetButton("MenuButton");
            //_cancelButton = _gameScreen.GetButton("CancelButton");

            _menuButton.clickable.clicked += PauseRequest;
            //_cancelButton.clickable.clicked += PauseRequest;
        }

        private void PauseRequest()
        {
            var entityClick = CreateInputEntity.Empty();
            entityClick.isPauseRequested = true;
            entityClick.isInput = true;
        }

        private void CloseMenu()
        {
            _uIService.Hide(_gameMenu).AsAsyncUnitUniTask();

            _gameMenu.pickingMode = PickingMode.Ignore;
            _mask.pickingMode = PickingMode.Ignore;
            _cancelButton.pickingMode = PickingMode.Ignore;
        }

        private void OpenMenu()
        {
            _uIService.Show(_gameMenu).AsAsyncUnitUniTask();

            _gameMenu.pickingMode = PickingMode.Position;
            _mask.pickingMode = PickingMode.Position;
            _cancelButton.pickingMode = PickingMode.Position;
        }

        private void OnDestroy()
        {
            _menuButton.clickable.clicked -= PauseRequest;
            //_cancelButton.clickable.clicked -= PauseRequest;
        }
    }
}