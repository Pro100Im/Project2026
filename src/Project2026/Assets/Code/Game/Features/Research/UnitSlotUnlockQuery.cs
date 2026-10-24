using Code.Infrastructure.Persistence;
using System.Collections.Generic;

namespace Code.Game.Features.Research
{
    public static class UnitSlotUnlockQuery
    {
        public static bool IsReady(UnitKind kind, List<GameEntity> nodes, IUserProgressService progress)
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];

                if (node.researchLevel.Value <= 0 || node.researchUnitSlotUnlock.Value != kind)
                    continue;

                if (BuildingReady(node, progress))
                    return true;
            }

            return false;
        }

        public static bool IsReady(UnitKind kind, List<TownEntity> nodes, List<TownEntity> buildings)
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];

                if (node.researchLevel.Value <= 0 || node.researchUnitSlotUnlock.Value != kind)
                    continue;

                if (BuildingReady(node, buildings))
                    return true;
            }

            return false;
        }

        private static bool BuildingReady(GameEntity node, IUserProgressService progress)
        {
            if (!node.hasResearchRequiredBuilding)
                return true;

            var requiredLevel = node.hasResearchRequiredBuildingLevel
                ? node.researchRequiredBuildingLevel.Value
                : 1;

            return progress.TryGetBuildingLevel(node.researchRequiredBuilding.Value, out var level)
                && level >= requiredLevel;
        }

        private static bool BuildingReady(TownEntity node, List<TownEntity> buildings)
        {
            if (!node.hasResearchRequiredBuilding)
                return true;

            var requiredLevel = node.hasResearchRequiredBuildingLevel
                ? node.researchRequiredBuildingLevel.Value
                : 1;
            var type = node.researchRequiredBuilding.Value;

            for (var i = 0; i < buildings.Count; i++)
            {
                var building = buildings[i];

                if (building.townBuildingType.Value != type)
                    continue;

                return building.townBuildingLevel.Value >= requiredLevel;
            }

            return false;
        }
    }
}
