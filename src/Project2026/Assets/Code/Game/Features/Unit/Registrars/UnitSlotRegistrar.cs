using Code.Game.Common.Entity;
using Code.Game.StaticData.Configs;
using UnityEngine;

namespace Code.Game.Features.Unit.Registrars
{
    public class UnitSlotRegistrar : MonoBehaviour
    {
        private void Awake()
        {
            for (var i = 0; i < UnitRoster.SlotCount; i++)
                CreateSlot(i);
        }

        private static void CreateSlot(int index)
        {
            var slot = CreateGameEntity.Empty();
            slot.AddUnitSlot(index);
        }
    }
}
