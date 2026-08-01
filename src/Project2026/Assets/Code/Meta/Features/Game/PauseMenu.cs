using Code.Game.Common.Entity;
using Code.Game.Common.UI;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Code.Meta.Features.Game
{
    public class PauseMenu : MonoBehaviour
    {
        private GameScreen _gameScreen;
        private UIService _uIService;

        private VisualElement _pauseMenu;
        private Image _mask;

        private Button _cancelButton;

        [Inject]
        public void Construct(UIService uIService, GameScreen gameScreen)
        {
            _gameScreen = gameScreen;
            _uIService = uIService;
        }

        private void Start()
        {
            _pauseMenu = _gameScreen.GetVisualElement("PauseMenu");
            _mask = _gameScreen.GetImage("Mask");
            _cancelButton = _gameScreen.GetButton("CancelButton");

            _cancelButton.clickable.clicked += PauseRequest;
        }

        private void PauseRequest()
        {
            var entityClick = CreateInputEntity.Empty();
            entityClick.isPauseRequested = true;
            entityClick.isInput = true;
        }

        public void CloseMenu()
        {
            _uIService.Hide(_pauseMenu).AsAsyncUnitUniTask();

            _pauseMenu.pickingMode = PickingMode.Ignore;
            _mask.pickingMode = PickingMode.Ignore;
            _cancelButton.pickingMode = PickingMode.Ignore;
        }

        public void OpenMenu()
        {
            _uIService.Show(_pauseMenu).AsAsyncUnitUniTask();

            _pauseMenu.pickingMode = PickingMode.Position;
            _mask.pickingMode = PickingMode.Position;
            _cancelButton.pickingMode = PickingMode.Position;
        }

        private void OnDestroy()
        {
            _cancelButton.clickable.clicked -= PauseRequest;
        }
    }
}