using Code.Common.Entity;
using Code.Game.StaticData.Configs;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Meta.Features.Game
{
    public class Game : MonoBehaviour
    {
        [SerializeField] private UIDocument _gameScreenDoc;
        [SerializeField] private EntityConfig _test;

        private VisualElement _gameScreen;

        private Button _startWaveButton;

        private void Awake()
        {
            var root = _gameScreenDoc.rootVisualElement;

            _gameScreen = root.Q<VisualElement>("GameScreen");

            _startWaveButton = root.Q<Button>("StartWaveButton");
            _startWaveButton.clickable.clicked += StartWave;

            //_exitButton = root.Q<Button>("ExitButton");
            //_exitButton.clickable.clicked += Exit;
        }

        private void StartWave()
        {
            var entity = CreateGameEntity.Empty();

            entity.isWaveStartRequsted = true;
        }

        private void OnDestroy()
        {
            _startWaveButton.clickable.clicked -= StartWave;
        }
    }
}