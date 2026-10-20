using Code.Game.Common.Entity;
using Code.Game.StaticData.Configs;
using Code.Game.StaticData.Property;
using UnityEngine;

namespace Code.Game.Features.Unit.Registrars
{
    public class UnitSlotRegistrar : MonoBehaviour
    {
        [SerializeField] private EntityConfig _firstUnitSlot;
        [SerializeField] private EntityConfig _secondUnitSlot;

        private void Awake()
        {
            CreateSlot(0, _firstUnitSlot);
            CreateSlot(1, _secondUnitSlot);
        }

        private static void CreateSlot(int index, EntityConfig config)
        {
            if (config == null)
                return;

            var slot = CreateGameEntity.Empty();
            slot.AddUnitSlot(index);
            slot.AddDefaultUnitConfig(config);

            var kind = config.GetProperty<UnitKindProperty>();

            if (kind != null)
                slot.AddUnitKind(kind.Kind);
        }
    }
}
