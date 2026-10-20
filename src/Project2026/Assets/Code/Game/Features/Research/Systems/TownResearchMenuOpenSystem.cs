using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Research.Systems
{
    public class TownResearchMenuOpenSystem : ReactiveSystem<TownEntity>
    {
        public TownResearchMenuOpenSystem() : base(Contexts.sharedInstance.town) { }

        protected override ICollector<TownEntity> GetTrigger(IContext<TownEntity> context) =>
            context.CreateCollector(TownMatcher.TownResearchMenuOpened.AddedOrRemoved());

        protected override bool Filter(TownEntity entity) => entity.hasTownResearchMenu;

        protected override void Execute(List<TownEntity> entities)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var menu = entities[i];

                if (menu.isTownResearchMenuOpened)
                    menu.townResearchMenu.Value.Show();
                else
                    menu.townResearchMenu.Value.Hide();
            }
        }
    }
}
