using Code.Game.Features.Town;
using Code.Game.StaticData.Configs;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Research.Systems
{
    public class TownGoldBonusSystem : IExecuteSystem
    {
        private readonly IGroup<TownEntity> _bonuses;
        private readonly IGroup<TownEntity> _buildings;
        private readonly List<TownEntity> _buffer = new(8);

        public TownGoldBonusSystem()
        {
            var town = Contexts.sharedInstance.town;
            _bonuses = town.GetGroup(TownMatcher
                .AllOf(
                    TownMatcher.ResearchBonus,
                    TownMatcher.GoldIncomeBonus));
            _buildings = town.GetGroup(TownMatcher
                .AllOf(
                    TownMatcher.TownBuilding,
                    TownMatcher.TownBuildingConfig));
        }

        public void Execute()
        {
            var bonus = _bonuses.GetSingleEntity();
            var multiplier = bonus == null ? 1f : 1f + bonus.goldIncomeBonus.Value;
            var buildings = _buildings.GetEntities(_buffer);

            for (var i = 0; i < buildings.Count; i++)
            {
                var building = buildings[i];

                if (building.townBuildingConfig.Value.Resource != TownResource.Gold)
                    continue;

                if (building.hasTownProductionMultiplier && building.townProductionMultiplier.Value == multiplier)
                    continue;

                if (building.hasTownProductionMultiplier)
                    building.ReplaceTownProductionMultiplier(multiplier);
                else
                    building.AddTownProductionMultiplier(multiplier);
            }
        }
    }
}
