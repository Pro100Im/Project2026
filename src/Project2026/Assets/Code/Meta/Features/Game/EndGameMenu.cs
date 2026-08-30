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
        private VisualElement _canvas;
        private Image _mask;

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
            _canvas = _gameScreen.GetVisualElement("Canvas");
            _gameOverMenu = _gameScreen.GetVisualElement("GameOverMenu");
            _mask = _gameOverMenu.Q<Image>("Mask");
            _restartButton = _gameOverMenu.Q<Button>("RestartButton");
            _townButton = _gameOverMenu.Q<Button>("TownButton");
            _exitButton = _gameOverMenu.Q<Button>("ExitButton");

            _restartButton.clickable.clicked += Restart;
            _townButton.clickable.clicked += Town;
            _exitButton.clickable.clicked += Exit;
        }

        private async void Town()
        {
            await _transitionScreen.Show();

            try
            {
                _cameraService.SetActiveTownCamera();
                _uIService.Hide(_canvas).Forget();

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
            await _sceneLoader.Load(_gameSceneName, LoadSceneMode.Additive, default, true);

            _sceneLoader.UnLoad(_gameSceneName).Forget();
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

            _gameOverMenu.pickingMode = PickingMode.Ignore;
            _mask.pickingMode = PickingMode.Ignore;

            _restartButton.pickingMode = PickingMode.Ignore;
            _townButton.pickingMode = PickingMode.Ignore;
            _exitButton.pickingMode = PickingMode.Ignore;
        }

        public void OpenMenu()
        {
            _uIService.Show(_gameOverMenu).AsAsyncUnitUniTask();

            _gameOverMenu.pickingMode = PickingMode.Position;
            _mask.pickingMode = PickingMode.Position;

            _restartButton.pickingMode = PickingMode.Position;
            _townButton.pickingMode = PickingMode.Position;
            _exitButton.pickingMode = PickingMode.Position;
        }

        private void OnDestroy()
        {
            if (_restartButton != null)
                _restartButton.clickable.clicked -= Restart;
            if (_townButton != null)
                _townButton.clickable.clicked -= Town;
            if (_exitButton != null)
                _exitButton.clickable.clicked -= Exit;
        }
    }
}