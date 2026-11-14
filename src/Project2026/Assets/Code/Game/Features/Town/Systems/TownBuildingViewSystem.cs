using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Town.Systems
{
    public class TownBuildingViewSystem : ReactiveSystem<TownEntity>
    {
        private readonly List<Transform> _changedBuffer = new(16);

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
                var level = building.townBuildingLevel.Value - 1;

                if (!building.isTownBuildingBuilt)
                {
                    building.townView.Value.SetViews(level);
                    continue;
                }

                building.townView.Value.SetViews(level, _changedBuffer);

                if (_changedBuffer.Count > 0)
                {
                    if (building.hasTownBuildEffectTargets)
                        building.ReplaceTownBuildEffectTargets(new List<Transform>(_changedBuffer));
                    else
                        building.AddTownBuildEffectTargets(new List<Transform>(_changedBuffer));
                }

                _changedBuffer.Clear();
            }
        }
    }
}
