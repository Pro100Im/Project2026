using Code.Game.Common.Entity;
using Code.Game.Common.UI;
using Code.Game.Common.UI.Transition;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VContainer;

namespace Code.Meta.Features.Game
{
    public class GameScreen : MonoBehaviour
    {
        [SerializeField] private string _gameSceneName = "Game";
        [SerializeField] private UIDocument _gameScreenDoc;

        private UIService _uIService;
        private TransitionScreen _transitionScreen;

        private VisualElement _root;
        private VisualElement _canvas;

        private Button _startWaveButton;
        private Button _menuButton;

        [Inject]
        public void Construct(UIService uIService, TransitionScreen transitionScreen)
        {
            _uIService = uIService;
            _transitionScreen = transitionScreen;
        }

        private void Awake()
        {
            _root = _gameScreenDoc.rootVisualElement;

            _canvas = _root.Q<VisualElement>("GameScreenCanvas");
            _startWaveButton = _root.Q<Button>("StartWaveButton");
            _menuButton = _root.Q<Button>("MenuButton");

            _startWaveButton.clickable.clicked += StartWave;
            _menuButton.clickable.clicked += PauseRequest;
        }

        private void Start()
        {
            _uIService.RegisterRoot(_root);
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnActiveSceneChanged(Scene arg0, Scene arg1)
        {
            if (!SceneManager.GetActiveScene().name.Equals(_gameSceneName))
            {
                _uIService.Hide(_canvas).Forget();

                return;
            }    

            _uIService.Show(_canvas).Forget();
            _transitionScreen.Hide().Forget();
        }

        public VisualElement GetRoot() => _root;

        public VisualElement GetVisualElement(string name) => _root.Q<VisualElement>(name);

        public Button GetButton(string name) => _root.Q<Button>(name);
        public Image GetImage(string name) => _root.Q<Image>(name);
        public Label GetLabel(string name) => _root.Q<Label>(name);

        private void StartWave()
        {
            var entity = CreateGameEntity.Empty();

            entity.isWaveStartRequsted = true;
        }

        private void PauseRequest()
        {
            var entityClick = CreateInputEntity.Empty();
            entityClick.isPauseRequested = true;
            entityClick.isInput = true;
        }

        private void OnDestroy()
        {
            _uIService?.UnregisterRoot(_root);
            _startWaveButton.clickable.clicked -= StartWave;
            _menuButton.clickable.clicked -= PauseRequest;
        }
    }
}
