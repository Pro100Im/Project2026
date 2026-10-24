using Code.Infrastructure.View.Registrars;
using Code.Meta.Features.Town;
using UnityEngine;

namespace Code.Game.Features.Town.Registrars
{
    public class TownEquipmentMenuRegistrar : TownEntityComponentRegistrar
    {
        [SerializeField] private TownEquipmentMenu _townEquipmentMenu;

        public override void RegisterComponents()
        {
            Entity.AddTownEquipmentMenu(_townEquipmentMenu);
        }

        public override void UnregisterComponents()
        {
            if (Entity.hasTownEquipmentMenu)
                Entity.RemoveTownEquipmentMenu();

            Entity.isTownEquipmentMenuOpened = false;
        }
    }
}
