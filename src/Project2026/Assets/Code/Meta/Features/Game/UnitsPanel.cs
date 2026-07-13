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

        private GameScreen _gameScreen;

        private Button _firstUnitSlotButton;

        [Inject]
        public void Construct(GameScreen gameScreen)
        {
            _gameScreen = gameScreen;
        }

        private void Start()
        {
            _firstUnitSlotButton = _gameScreen.GetButton("UnitButton");

            _firstUnitSlotButton.clickable.clicked += SpawnFistSlotUnit;
        }

        private void SpawnFistSlotUnit()
        {
            var entity = CreateGameEntity.Empty();
            var unitSize = _firstUnitSlot.GetProperty<UnitSizeProperty>().Size;

            entity.AddUnitSize(unitSize);
            entity.AddEntityConfig(_firstUnitSlot);
            entity.isSpawnRequsted = true;
            entity.isPlayer = true;
        }

        private void OnDestroy()
        {
            _firstUnitSlotButton.clickable.clicked -= SpawnFistSlotUnit;
        }
    }
}