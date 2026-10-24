using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Town.Systems
{
    public class TownEquipmentMenuOpenSystem : ReactiveSystem<TownEntity>
    {
        public TownEquipmentMenuOpenSystem() : base(Contexts.sharedInstance.town) { }

        protected override ICollector<TownEntity> GetTrigger(IContext<TownEntity> context) =>
            context.CreateCollector(TownMatcher.TownEquipmentMenuOpened.AddedOrRemoved());

        protected override bool Filter(TownEntity entity) => entity.hasTownEquipmentMenu;

        protected override void Execute(List<TownEntity> entities)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var menu = entities[i];

                if (menu.isTownEquipmentMenuOpened)
                    menu.townEquipmentMenu.Value.Show();
                else
                    menu.townEquipmentMenu.Value.Hide();
            }
        }
    }
}
