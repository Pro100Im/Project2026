using Code.Game.Common.Entity;
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

        private void SpawnFirstSlotUnit() => RequestSpawn(0);

        private void SpawnSecondSlotUnit() => RequestSpawn(1);

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
