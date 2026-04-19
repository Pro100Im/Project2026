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
        [SerializeField] private TowerMenuScreen _towerMenuScreen;

        private UIService _uIService;

        private VisualElement _root;

        private Button _startWaveButton;

        [Inject]
        public void Construct(UIService uIService)
        {
            _uIService = uIService;
        }

        private void Awake()
        {
            _root = _gameScreenDoc.rootVisualElement;

            _startWaveButton = _root.Q<Button>("StartWaveButton");
            _startWaveButton.clickable.clicked += StartWave;
        }

        public VisualElement GetRoot() => _root;

        public VisualElement GetVisualElement(string name) => _root.Q<VisualElement>(name);

        public Button GetButton(string name) => _root.Q<Button>(name);

        private void StartWave()
        {
            var entity = CreateGameEntity.Empty();

            entity.isWaveStartRequsted = true;
        }

        public void OpenTowerBuildMenu(Vector2 screenPos, GameEntity entity)
        {
            _towerMenuScreen.Open(screenPos, entity);
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