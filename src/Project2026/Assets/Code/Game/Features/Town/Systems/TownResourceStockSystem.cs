using Code.Game.StaticData.Configs;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Town.Systems
{
    public class TownResourceStockSystem : IExecuteSystem
    {
        private readonly IGroup<TownEntity> _stock;
        private readonly IGroup<TownEntity> _gains;

        private readonly List<TownEntity> _gainsBuffer = new(8);

        public TownResourceStockSystem()
        {
            var town = Contexts.sharedInstance.town;

            _stock = town.GetGroup(TownMatcher.TownResources);
            _gains = town.GetGroup(TownMatcher
                .AllOf(
                    TownMatcher.TownResourceGained,
                    TownMatcher.TownBuildingConfig));
        }

        public void Execute()
        {
            var stock = _stock.GetSingleEntity();

            if (stock == null)
                return;

            var gains = _gains.GetEntities(_gainsBuffer);

            for (var i = 0; i < gains.Count; i++)
            {
                var building = gains[i];
                var amount = building.townResourceGained.Value;

                switch (building.townBuildingConfig.Value.Resource)
                {
                    case TownResource.Gold:
                        stock.ReplaceTownGold(stock.townGold.Value + amount);
                        break;
                    case TownResource.Stone:
                        stock.ReplaceTownStone(stock.townStone.Value + amount);
                        break;
                    case TownResource.Crystal:
                        stock.ReplaceTownCrystal(stock.townCrystal.Value + amount);
                        break;
                    case TownResource.Wood:
                        stock.ReplaceTownWood(stock.townWood.Value + amount);
                        break;
                    case TownResource.Knowledge:
                        stock.ReplaceTownKnowledge(stock.townKnowledge.Value + amount);
                        break;
                }
            }
        }
    }
}
