using Code.Game.Common.Time;
using Code.Game.StaticData.Configs;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Town.Systems
{
    public class TownProductionTickSystem : IExecuteSystem
    {
        private readonly ITimeService _timeService;

        private readonly IGroup<TownEntity> _buildings;
        private readonly List<TownEntity> _buildingsBuffer = new(8);

        public TownProductionTickSystem(ITimeService timeService)
        {
            _timeService = timeService;

            _buildings = Contexts.sharedInstance.town.GetGroup(TownMatcher
                .AllOf(
                    TownMatcher.TownBuilding,
                    TownMatcher.TownBuildingConfig,
                    TownMatcher.TownBuildingLevel,
                    TownMatcher.TownBuildingWorkers,
                    TownMatcher.TownProductionCooldown));
        }

        public void Execute()
        {
            var buildings = _buildings.GetEntities(_buildingsBuffer);

            for (var i = 0; i < buildings.Count; i++)
            {
                var building = buildings[i];

                if (building.townBuildingLevel.Value <= 0)
                    continue;

                if (building.townBuildingConfig.Value.Resource == TownResource.None)
                    continue;

                if (!building.townBuildingConfig.Value.TryGetProduction(
                        building.townBuildingLevel.Value,
                        out var interval,
                        out var amount))
                    continue;

                var cooldown = building.townProductionCooldown.Value - _timeService.DeltaTime;

                if (cooldown > 0f)
                {
                    building.ReplaceTownProductionCooldown(cooldown);
                    continue;
                }

                building.ReplaceTownProductionCooldown(interval);

                var gain = building.townBuildingWorkers.Value * amount;

                if (gain <= 0)
                    continue;

                if (building.hasTownResourceGained)
                    building.ReplaceTownResourceGained(building.townResourceGained.Value + gain);
                else
                    building.AddTownResourceGained(gain);
            }
        }
    }
}
