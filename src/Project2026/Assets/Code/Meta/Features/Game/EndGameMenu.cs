using Code.Game.Common.Cameras;
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
    public class EndGameMenu : MonoBehaviour
    {
        [SerializeField] private string _menuSceneName = "HomeScreen";
        [SerializeField] private string _gameSceneName = "Game";
        [SerializeField] private string _townSceneName = "Town";

        private GameScreen _gameScreen;
        private UIService _uIService;
        private TransitionScreen _transitionScreen;
        private ISceneLoader _sceneLoader;
        private ICameraService _cameraService;

        private VisualElement _gameOverMenu;

        private Button _restartButton;
        private Button _townButton;
        private Button _exitButton;

        [Inject]
        public void Construct(UIService uIService, GameScreen gameScreen, ICameraService cameraService, ISceneLoader sceneLoader, TransitionScreen transitionScreen)
        {
            _gameScreen = gameScreen;
            _uIService = uIService;
            _transitionScreen = transitionScreen;
            _sceneLoader = sceneLoader;
            _cameraService = cameraService;
        }

        private void Start()
        {
            _gameOverMenu = _gameScreen.GetVisualElement("GameOverMenu");
            _restartButton = _gameOverMenu.Q<Button>("RestartButton");
            _townButton = _gameOverMenu.Q<Button>("TownButton");
            _exitButton = _gameOverMenu.Q<Button>("ExitButton");

            _restartButton.clickable.clicked += Restart;
            _townButton.clickable.clicked += Town;
            _exitButton.clickable.clicked += Exit;

            SceneManager.activeSceneChanged += OnActiveSceneChanged;

            _uIService.Hide(_gameOverMenu).Forget();
        }

        private async void Town()
        {
            await _transitionScreen.Show();

            try
            {
                CloseMenu();

                _cameraService.SetActiveTownCamera();

                var townScene = SceneManager.GetSceneByName(_townSceneName);

                if (townScene.IsValid())
                    SceneManager.SetActiveScene(townScene);
            }
            catch
            {

            }
        }

        private async void Restart()
        {
            await _transitionScreen.Show();
            await _sceneLoader.UnLoad(_gameSceneName);

            _sceneLoader.Load(_gameSceneName, LoadSceneMode.Additive, default, true).Forget();
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
            _uIService.Hide(_gameOverMenu).AsAsyncUnitUniTask();
        }

        public void OpenMenu()
        {
            _uIService.Show(_gameOverMenu).AsAsyncUnitUniTask();
        }

        private void OnActiveSceneChanged(Scene _, Scene __)
        {
            if (!SceneManager.GetActiveScene().name.Equals(_gameSceneName))
                return;

            var session = Contexts.sharedInstance.game
                .GetGroup(GameMatcher.GameSession)
                .GetSingleEntity();

            if (session != null && session.isForcedPause)
                OpenMenu();
        }

        private void OnDestroy()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;

            if (_restartButton != null)
                _restartButton.clickable.clicked -= Restart;
            if (_townButton != null)
                _townButton.clickable.clicked -= Town;
            if (_exitButton != null)
                _exitButton.clickable.clicked -= Exit;
        }
    }
}
