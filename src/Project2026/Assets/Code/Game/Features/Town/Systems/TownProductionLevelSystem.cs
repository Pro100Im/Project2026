using Code.Game.StaticData.Configs;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Town.Systems
{
    public class TownProductionLevelSystem : IExecuteSystem
    {
        private readonly IGroup<TownEntity> _buildings;
        private readonly List<TownEntity> _buildingsBuffer = new(8);
        private readonly Dictionary<int, int> _armedLevels = new(8);

        public TownProductionLevelSystem()
        {
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
                var level = building.townBuildingLevel.Value;

                if (_armedLevels.TryGetValue(building.creationIndex, out var armedLevel) && armedLevel == level)
                    continue;

                _armedLevels[building.creationIndex] = level;

                if (level <= 0 || building.townBuildingConfig.Value.Resource == TownResource.None)
                    continue;

                if (!building.townBuildingConfig.Value.TryGetProduction(level, out var interval, out _))
                    continue;

                building.ReplaceTownProductionCooldown(interval);
            }
        }
    }
}
