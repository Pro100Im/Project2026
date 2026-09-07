using Code.Game.Common.Entity;
using Code.Game.Common.UI;
using Code.Game.Common.UI.Transition;
using Code.Infrastructure.Loading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VContainer;

namespace Code.Meta.Features.Game
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private string _menuSceneName = "HomeScreen";
        [SerializeField] private string _gameSceneName = "Game";

        private GameScreen _gameScreen;
        private UIService _uIService;
        private TransitionScreen _transitionScreen;
        private ISceneLoader _sceneLoader;

        private VisualElement _pauseMenu;

        private Button _cancelButton;
        private Button _exitButton;

        [Inject]
        public void Construct(UIService uIService, GameScreen gameScreen, ISceneLoader sceneLoader, TransitionScreen transitionScreen)
        {
            _gameScreen = gameScreen;
            _uIService = uIService;
            _transitionScreen = transitionScreen;
            _sceneLoader = sceneLoader;
        }

        private void Start()
        {
            _pauseMenu = _gameScreen.GetVisualElement("PauseMenu");
            _cancelButton = _pauseMenu.Q<Button>("CancelButton");
            _exitButton = _pauseMenu.Q<Button>("ExitButton");

            _cancelButton.clickable.clicked += PauseRequest;
            _exitButton.clickable.clicked += Exit;

            _uIService.Hide(_pauseMenu).Forget();
        }

        private void PauseRequest()
        {
            var entityClick = CreateInputEntity.Empty();
            entityClick.isPauseRequested = true;
            entityClick.isInput = true;
        }

        public void Exit()
        {
            ExitAsync().Forget();
        }

        private async UniTaskVoid ExitAsync()
        {
            await _transitionScreen.Show();
            await _sceneLoader.Load(_menuSceneName, LoadSceneMode.Additive, default, true); 

            _sceneLoader.UnLoad(_gameSceneName).Forget();
        }

        public void CloseMenu()
        {
            _uIService.Hide(_pauseMenu).AsAsyncUnitUniTask();
        }

        public void OpenMenu()
        {
            _uIService.Show(_pauseMenu).AsAsyncUnitUniTask();
        }

        private void OnDestroy()
        {
            if (_cancelButton != null)
                _cancelButton.clickable.clicked -= PauseRequest;
            if (_exitButton != null)
                _exitButton.clickable.clicked -= Exit;
        }
    }
}
