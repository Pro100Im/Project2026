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
        private Label _waveCallTitle;
        private Label _waveEnemiesRemaining;

        private int _displayedWave;
        private int _displayedTotal;
        private int _displayedRemaining = -1;
        private bool _displayedCanCall = true;

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
            _waveCallTitle = _root.Q<Label>("WaveCallTitle");
            _waveEnemiesRemaining = _root.Q<Label>("WaveEnemiesRemaining");

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

        public void SetWaveHud(int wave, int total, int remaining, bool canCall)
        {
            if (wave != _displayedWave || total != _displayedTotal)
            {
                _displayedWave = wave;
                _displayedTotal = total;
                _waveCallTitle.text = $"{wave} / {total}";
            }

            if (remaining != _displayedRemaining)
            {
                _displayedRemaining = remaining;
                _waveEnemiesRemaining.text = remaining.ToString();
            }

            if (canCall != _displayedCanCall)
            {
                _displayedCanCall = canCall;
                _startWaveButton.SetEnabled(canCall);
            }
        }

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
