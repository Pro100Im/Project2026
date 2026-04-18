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
        private VisualElement _gameScreen;

        private Button _startWaveButton;

        [Inject]
        public void Construct(UIService uIService)
        {
            _uIService = uIService;
        }

        private void Awake()
        {
            _root = _gameScreenDoc.rootVisualElement;
            _gameScreen = _root.Q<VisualElement>("GameScreen");

            _startWaveButton = _root.Q<Button>("StartWaveButton");
            _startWaveButton.clickable.clicked += StartWave;

            //_exitButton = root.Q<Button>("ExitButton");
            //_exitButton.clickable.clicked += Exit;
        }

        public VisualElement GetVisualElement(string name) => _root.Q<VisualElement>(name);

        public Button GetButton(string name) => _root.Q<Button>(name);

        private void StartWave()
        {
            var entity = CreateGameEntity.Empty();

            entity.isWaveStartRequsted = true;
        }

        private void OnDestroy()
        {
            _startWaveButton.clickable.clicked -= StartWave;
        }

        public bool IsPointerOverUI(Vector2 screenPos)
        {
            return _uIService.IsPointerOverUI(screenPos, _root);
        }
    }
}