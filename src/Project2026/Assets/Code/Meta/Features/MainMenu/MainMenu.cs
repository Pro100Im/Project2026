using Code.Game.Common.UI;
using Code.Game.Common.UI.Transition;
using Code.Infrastructure.Loading;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VContainer;

namespace Code.Meta.UI.MainMenu
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private string _gameSceneName = "Game";
        [SerializeField] private string _homeScreenSceneName = "HomeScreen";
        [SerializeField] private string _townSceneName = "Town";
        [Space]
        [SerializeField] private UIDocument _mainMenuDoc;
        [Space]
        [SerializeField] private InputActionMap _pressAnyBtn;

        private VisualElement _canvas;
        private VisualElement _mainMenu;
        private VisualElement _intro;

        private Button _quickMatchButton;
        private Button _exitButton;
        private Button _townButton;

        private ISceneLoader _sceneLoader;
        private TransitionScreen _transitionScreen;
        private UIService _uIService;

        public IObserver<InputControl> OnAnyButton { get; private set; }

        [Inject]
        private void Construct(ISceneLoader sceneLoader, TransitionScreen transitionScreen, UIService uIService)
        {
            _sceneLoader = sceneLoader;
            _transitionScreen = transitionScreen;
            _uIService = uIService;
        }

        private void Awake()
        {
            var root = _mainMenuDoc.rootVisualElement;

            _canvas = root.Q<VisualElement>("Canvas");

            _intro = root.Q<VisualElement>("Intro");

            _mainMenu = root.Q<VisualElement>("MainMenu");

            _quickMatchButton = root.Q<Button>("QuickPlayButton");
            _quickMatchButton.clickable.clicked += StartGame;

            _townButton = root.Q<Button>("TownButton");
            _townButton.clickable.clicked += EnterTown;

            _exitButton = root.Q<Button>("ExitButton");
            _exitButton.clickable.clicked += Exit;

            _pressAnyBtn.actionTriggered += OnAnyButtonPress;
            _pressAnyBtn.Enable();
        }

        private async void EnterTown()
        {
            await _transitionScreen.Show();

            try
            {
                var townScene = SceneManager.GetSceneByName(_townSceneName);

                if (townScene.IsValid())
                    SceneManager.SetActiveScene(townScene);

                await _uIService.Hide(_canvas);
            }
            finally
            {
                await _transitionScreen.Hide();
            }
        }

        private void Start()
        {
            _transitionScreen.Hide().AsTask();
        }

        private void OnAnyButtonPress(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                _pressAnyBtn.Disable();
                _pressAnyBtn.actionTriggered -= OnAnyButtonPress;

                _uIService.Hide(_intro).AsTask();
                _uIService.Show(_mainMenu).AsTask();

                _quickMatchButton.pickingMode = PickingMode.Position;
                _townButton.pickingMode = PickingMode.Position;
                _exitButton.pickingMode = PickingMode.Position;
            }
        }

        private async void StartGame()
        {
            await _transitionScreen.Show();
            await _sceneLoader.Load(_gameSceneName, LoadSceneMode.Additive);   
            
            _sceneLoader.UnLoad(_homeScreenSceneName).Forget();
        }

        private void Exit()
        {
            Application.Quit();
        }

        private void OnDestroy()
        {
            _quickMatchButton.clickable.clicked -= StartGame;
            _townButton.clickable.clicked -= EnterTown;
            _exitButton.clickable.clicked -= Exit;
        }
    }
}