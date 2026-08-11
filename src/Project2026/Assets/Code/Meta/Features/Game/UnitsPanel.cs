using Assets.Code.Game.StaticData.Property;
using Code.Game.Common.Entity;
using Code.Game.StaticData.Configs;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Code.Meta.Features.Game
{
    public class UnitsPanel : MonoBehaviour
    {
        [SerializeField] private EntityConfig _firstUnitSlot;
        [SerializeField] private EntityConfig _secondUnitSlot;

        private GameScreen _gameScreen;

        private Button _firstUnitSlotButton;
        private Button _secondUnitSlotButton;

        [Inject]
        public void Construct(GameScreen gameScreen)
        {
            _gameScreen = gameScreen;
        }

        private void Start()
        {
            _firstUnitSlotButton = _gameScreen.GetButton("UnitButton1");
            _secondUnitSlotButton = _gameScreen.GetButton("UnitButton2");

            _firstUnitSlotButton.clickable.clicked += SpawnFirstSlotUnit;
            _secondUnitSlotButton.clickable.clicked += SpawnSecondSlotUnit;
        }

        private void SpawnFirstSlotUnit()
        {
            var entity = CreateGameEntity.Empty();
            var unitSize = _firstUnitSlot.GetProperty<UnitSizeProperty>().Size;

            entity.AddUnitSize(unitSize);
            entity.AddEntityConfig(_firstUnitSlot);
            entity.isSpawnRequsted = true;
            entity.isPlayer = true;
        }

        private void SpawnSecondSlotUnit()
        {
            var entity = CreateGameEntity.Empty();
            var unitSize = _secondUnitSlot.GetProperty<UnitSizeProperty>().Size;

            entity.AddUnitSize(unitSize);
            entity.AddEntityConfig(_secondUnitSlot);
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