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
        private VisualElement _root;
        private VisualElement _spellButtons;

        [Inject]
        public void Construct(GameScreen gameScreen)
        {
            _gameScreen = gameScreen;
        }

        private void Start()
        {
            _root = _gameScreen.GetRoot();
            _spellButtons = _gameScreen.GetVisualElement("SpellButtons");

            _freezeAbilityButton = _gameScreen.GetButton("SpellButton");
            _freezeAbilityButton.clickable.clicked += SelectFreezeAbility;

            _root.RegisterCallback<PointerDownEvent>(OnRootPointerDown, TrickleDown.TrickleDown);
        }

        private void SelectFreezeAbility()
        {
            var request = CreateGameEntity.Empty();

            request.AddEntityConfig(_freezeAbilitySlot);
            request.isAbilitySelectRequest = true;
        }

        private void OnRootPointerDown(PointerDownEvent evt)
        {
            var element = evt.target as VisualElement;

            if (element == null)
                return;

            var button = element as Button ?? element.GetFirstAncestorOfType<Button>();

            if (button == null)
                return;

            if (_spellButtons != null && _spellButtons.Contains(button))
                return;

            var request = CreateGameEntity.Empty();
            request.isAbilityCancelRequest = true;
        }

        private void OnDestroy()
        {
            if (_freezeAbilityButton != null)
                _freezeAbilityButton.clickable.clicked -= SelectFreezeAbility;

            if (_root != null)
                _root.UnregisterCallback<PointerDownEvent>(OnRootPointerDown, TrickleDown.TrickleDown);
        }
    }
}
