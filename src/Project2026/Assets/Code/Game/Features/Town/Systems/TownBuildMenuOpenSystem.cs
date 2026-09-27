using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Town.Systems
{
    public class TownBuildMenuOpenSystem : ReactiveSystem<TownEntity>
    {
        public TownBuildMenuOpenSystem() : base(Contexts.sharedInstance.town) { }

        protected override ICollector<TownEntity> GetTrigger(IContext<TownEntity> context) =>
            context.CreateCollector(TownMatcher.TownBuildMenuOpened.AddedOrRemoved());

        protected override bool Filter(TownEntity entity) => entity.hasTownBuildMenu;

        protected override void Execute(List<TownEntity> entities)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var menu = entities[i];

                if (menu.isTownBuildMenuOpened)
                    menu.townBuildMenu.Value.Show();
                else
                    menu.townBuildMenu.Value.Hide();
            }
        }
    }
}
