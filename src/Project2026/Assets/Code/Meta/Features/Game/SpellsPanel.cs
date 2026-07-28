using Code.Game.Common.Entity;
using Code.Game.StaticData.Configs;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Code.Meta.Features.Game
{
    public class SpellsPanel : MonoBehaviour
    {
        [SerializeField] private EntityConfig _freezeAbilitySlot;

        private GameScreen _gameScreen;
        private Button _freezeAbilityButton;

        [Inject]
        public void Construct(GameScreen gameScreen)
        {
            _gameScreen = gameScreen;
        }

        private void Start()
        {
            _freezeAbilityButton = _gameScreen.GetButton("SpellButton");
            _freezeAbilityButton.clickable.clicked += SelectFreezeAbility;
        }

        private void SelectFreezeAbility()
        {
            var request = CreateGameEntity.Empty();

            request.AddEntityConfig(_freezeAbilitySlot);
            request.isAbilitySelectRequest = true;
        }

        private void OnDestroy()
        {
            if (_freezeAbilityButton != null)
                _freezeAbilityButton.clickable.clicked -= SelectFreezeAbility;
        }
    }
}
