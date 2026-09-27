using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Town.Systems
{
    public class TownBuildingViewSystem : ReactiveSystem<TownEntity>
    {
        public TownBuildingViewSystem() : base(Contexts.sharedInstance.town) { }

        protected override ICollector<TownEntity> GetTrigger(IContext<TownEntity> context) =>
            context.CreateCollector(TownMatcher.TownBuildingLevel.Added());

        protected override bool Filter(TownEntity entity) =>
            entity.hasTownView && entity.hasTownBuildingLevel && entity.townBuildingLevel.Value > 0;

        protected override void Execute(List<TownEntity> entities)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var building = entities[i];

                building.townView.Value.SetViews(building.townBuildingLevel.Value - 1);
            }
        }
    }
}
