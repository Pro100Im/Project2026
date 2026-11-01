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
        private VisualElement _victoryMenu;

        private Button _gameOverRestartButton;
        private Button _gameOverTownButton;
        private Button _gameOverExitButton;

        private Button _victoryRestartButton;
        private Button _victoryTownButton;
        private Button _victoryExitButton;

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
            _victoryMenu = _gameScreen.GetVisualElement("VictoryMenu");

            _gameOverRestartButton = _gameOverMenu.Q<Button>("RestartButton");
            _gameOverTownButton = _gameOverMenu.Q<Button>("TownButton");
            _gameOverExitButton = _gameOverMenu.Q<Button>("ExitButton");

            _victoryRestartButton = _victoryMenu.Q<Button>("RestartButton");
            _victoryTownButton = _victoryMenu.Q<Button>("TownButton");
            _victoryExitButton = _victoryMenu.Q<Button>("ExitButton");

            _gameOverRestartButton.clickable.clicked += Restart;
            _gameOverTownButton.clickable.clicked += Town;
            _gameOverExitButton.clickable.clicked += Exit;

            _victoryRestartButton.clickable.clicked += Restart;
            _victoryTownButton.clickable.clicked += Town;
            _victoryExitButton.clickable.clicked += Exit;

            SceneManager.activeSceneChanged += OnActiveSceneChanged;

            _uIService.Hide(_gameOverMenu).Forget();
            _uIService.Hide(_victoryMenu).Forget();
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
            _uIService.Hide(_victoryMenu).AsAsyncUnitUniTask();
        }

        public void OpenMenu()
        {
            _uIService.Hide(_victoryMenu).AsAsyncUnitUniTask();
            _uIService.Show(_gameOverMenu).AsAsyncUnitUniTask();
        }

        public void OpenVictory()
        {
            _uIService.Hide(_gameOverMenu).AsAsyncUnitUniTask();
            _uIService.Show(_victoryMenu).AsAsyncUnitUniTask();
        }

        private void OnActiveSceneChanged(Scene _, Scene __)
        {
            if (!SceneManager.GetActiveScene().name.Equals(_gameSceneName))
                return;

            var session = Contexts.sharedInstance.game
                .GetGroup(GameMatcher.GameSession)
                .GetSingleEntity();

            if (session == null || !session.isForcedPause)
                return;

            if (session.isVictory)
                OpenVictory();
            else
                OpenMenu();
        }

        private void OnDestroy()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;

            if (_gameOverRestartButton != null)
                _gameOverRestartButton.clickable.clicked -= Restart;
            if (_gameOverTownButton != null)
                _gameOverTownButton.clickable.clicked -= Town;
            if (_gameOverExitButton != null)
                _gameOverExitButton.clickable.clicked -= Exit;

            if (_victoryRestartButton != null)
                _victoryRestartButton.clickable.clicked -= Restart;
            if (_victoryTownButton != null)
                _victoryTownButton.clickable.clicked -= Town;
            if (_victoryExitButton != null)
                _victoryExitButton.clickable.clicked -= Exit;
        }
    }
}
