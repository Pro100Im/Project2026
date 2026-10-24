using Code.Game.Common.Entity;
using Code.Game.StaticData.Configs;
using Entitas;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Code.Meta.Features.Game
{
    public class UnitsPanel : MonoBehaviour
    {
        private const int SlotCount = UnitRoster.SlotCount;

        private GameScreen _gameScreen;
        private UnitRoster _roster;

        private readonly Button[] _buttons = new Button[SlotCount];
        private readonly Image[] _icons = new Image[SlotCount];
        private readonly Action[] _clicks = new Action[SlotCount];

        private IGroup<GameEntity> _slots;
        private readonly List<GameEntity> _slotBuffer = new(4);

        [Inject]
        public void Construct(GameScreen gameScreen, UnitRoster roster)
        {
            _gameScreen = gameScreen;
            _roster = roster;
        }

        private void Start()
        {
            for (var i = 0; i < SlotCount; i++)
            {
                var button = _gameScreen.GetButton($"UnitButton{i + 1}");
                _buttons[i] = button;
                _icons[i] = button.Q<Image>("Icon");

                var slot = i;
                _clicks[i] = () => Spawn(slot);
                button.clickable.clicked += _clicks[i];
            }

            _slots = Contexts.sharedInstance.game.GetGroup(GameMatcher.UnitSlot);
        }

        private void Update()
        {
            for (var i = 0; i < SlotCount; i++)
                Apply(i);
        }

        private void Apply(int index)
        {
            var icon = _icons[index];

            if (icon == null)
                return;

            var sprite = _roster != null ? _roster.LockedIcon : null;

            if (TryGetSlot(index, out var slot) && slot.isUnitSlotAvailable && slot.hasUnitIcon)
                sprite = slot.unitIcon.Value;

            if (sprite != null)
                icon.style.backgroundImage = new StyleBackground(sprite);
        }

        private void Spawn(int index)
        {
            if (!TryGetSlot(index, out var slot) || !slot.isUnitSlotAvailable)
                return;

            RequestSpawn(index);
        }

        private bool TryGetSlot(int index, out GameEntity slot)
        {
            slot = null;

            if (_slots == null)
                return false;

            var slots = _slots.GetEntities(_slotBuffer);

            for (var i = 0; i < slots.Count; i++)
            {
                if (slots[i].unitSlot.Value != index)
                    continue;

                slot = slots[i];
                return true;
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
            for (var i = 0; i < _buttons.Length; i++)
            {
                if (_buttons[i] == null || _clicks[i] == null)
                    continue;

                _buttons[i].clickable.clicked -= _clicks[i];
            }
        }
    }
}
