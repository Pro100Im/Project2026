using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Code.Meta.Features.Game
{
    public class UnitsPanel : MonoBehaviour
    {
        private GameScreen _gameScreen;

        private Button _firstUnitSlotButton;
        private Button _secondUnitSlotButton;
        private Image _firstIcon;
        private Image _secondIcon;
        private StyleBackground _firstUnlockedIcon;
        private StyleBackground _secondUnlockedIcon;
        private StyleBackground _lockedIcon;

        private IGroup<GameEntity> _slots;
        private readonly List<GameEntity> _slotBuffer = new(4);

        [Inject]
        public void Construct(GameScreen gameScreen)
        {
            _gameScreen = gameScreen;
        }

        private void Start()
        {
            _firstUnitSlotButton = _gameScreen.GetButton("UnitButton1");
            _secondUnitSlotButton = _gameScreen.GetButton("UnitButton2");

            _firstIcon = _firstUnitSlotButton.Q<Image>("Icon");
            _secondIcon = _secondUnitSlotButton.Q<Image>("Icon");
            _firstUnlockedIcon = _firstIcon.style.backgroundImage;
            _secondUnlockedIcon = _secondIcon.style.backgroundImage;
            _lockedIcon = _gameScreen.GetButton("StubButton").Q<Image>("Icon").style.backgroundImage;

            _slots = Contexts.sharedInstance.game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.UnitSlot,
                    GameMatcher.UnitKind));

            _firstUnitSlotButton.clickable.clicked += SpawnFirstSlotUnit;
            _secondUnitSlotButton.clickable.clicked += SpawnSecondSlotUnit;
        }

        private void Update()
        {
            if (_firstIcon == null || _secondIcon == null)
                return;

            var firstAvailable = IsAvailable(0);
            var secondAvailable = IsAvailable(1);

            _firstIcon.style.backgroundImage = firstAvailable ? _firstUnlockedIcon : _lockedIcon;
            _secondIcon.style.backgroundImage = secondAvailable ? _secondUnlockedIcon : _lockedIcon;
        }

        private void SpawnFirstSlotUnit()
        {
            if (!IsAvailable(0))
                return;

            RequestSpawn(0);
        }

        private void SpawnSecondSlotUnit()
        {
            if (!IsAvailable(1))
                return;

            RequestSpawn(1);
        }

        private bool IsAvailable(int index)
        {
            if (_slots == null)
                return false;

            var slots = _slots.GetEntities(_slotBuffer);

            for (var i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];

                if (slot.unitSlot.Value == index)
                    return slot.isUnitSlotAvailable;
            }

            return false;
        }

        private static void RequestSpawn(int slot)
        {
            var entity = CreateGameEntity.Empty();
            entity.AddSpawnSlot(slot);
            entity.isSpawnRequsted = true;
            entity.isPlayer = true;
        }

        private void OnDestroy()
        {
            if (_firstUnitSlotButton != null)
                _firstUnitSlotButton.clickable.clicked -= SpawnFirstSlotUnit;
            if (_secondUnitSlotButton != null)
                _secondUnitSlotButton.clickable.clicked -= SpawnSecondSlotUnit;
        }
    }
}
