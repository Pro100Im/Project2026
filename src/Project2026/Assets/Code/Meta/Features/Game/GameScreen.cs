using Code.Game.Common.Entity;
using Code.Game.Common.UI;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Code.Meta.Features.Game
{
    public class GameScreen : MonoBehaviour
    {
        [SerializeField] private UIDocument _gameScreenDoc;

        private UIService _uIService;

        private VisualElement _root;

        private Button _startWaveButton;
        private Button _menuButton;

        [Inject]
        public void Construct(UIService uIService)
        {
            _uIService = uIService;
        }

        private void Awake()
        {
            _root = _gameScreenDoc.rootVisualElement;

            _startWaveButton = _root.Q<Button>("StartWaveButton");
            _menuButton = _root.Q<Button>("MenuButton");

            _startWaveButton.clickable.clicked += StartWave;
            _menuButton.clickable.clicked += PauseRequest;
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

        public bool IsPointerOverUI(Vector2 screenPos)
        {
            return _uIService.IsPointerOverUI(screenPos, _root);
        }

        private void OnDestroy()
        {
            _startWaveButton.clickable.clicked -= StartWave;
            _menuButton.clickable.clicked -= PauseRequest;
        }
    }
}