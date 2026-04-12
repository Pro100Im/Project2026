using Code.Game.Common.UI;
using Code.Game.Common.UI.Transition;
using Code.Infrastructure.Loading;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using VContainer;

namespace Code.Meta.UI.MainMenu
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private string _gameSceneName = "Game";
        [Space]
        [SerializeField] private UIDocument _mainMenuDoc;
        [Space]
        [SerializeField] private InputActionMap _pressAnyBtn;

        private VisualElement _mainMenu;
        private VisualElement _intro;

        private Button _quickMatchButton;
        private Button _exitButton;
        private Button _cancelSearchingButton;

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

            _intro = root.Q<VisualElement>("Intro");

            _mainMenu = root.Q<VisualElement>("MainMenu");

            _quickMatchButton = root.Q<Button>("QuickPlayButton");
            _quickMatchButton.clickable.clicked += EnterNetworkBattleLoadingState;

            _exitButton = root.Q<Button>("ExitButton");
            _exitButton.clickable.clicked += Exit;

            _pressAnyBtn.actionTriggered += OnAnyButtonPress;
            _pressAnyBtn.Enable();
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
                _exitButton.pickingMode = PickingMode.Position;
            }
        }

        private async void EnterNetworkBattleLoadingState()
        {
            await _transitionScreen.Show();

            _sceneLoader.Load(_gameSceneName);       
        }

        private void Exit()
        {

        }

        private void OnDestroy()
        {
            _quickMatchButton.clickable.clicked -= EnterNetworkBattleLoadingState;
            _exitButton.clickable.clicked -= Exit;
        }
    }
}