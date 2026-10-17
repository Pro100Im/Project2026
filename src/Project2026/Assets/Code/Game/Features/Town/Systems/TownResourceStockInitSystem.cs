using Code.Game.Common.Entity;
using Entitas;

namespace Code.Game.Features.Town.Systems
{
    public class TownResourceStockInitSystem : IInitializeSystem
    {
        public void Initialize()
        {
            var existing = Contexts.sharedInstance.town.GetGroup(TownMatcher.TownResources);

            if (existing.count > 0)
                return;

            var stock = CreateTownEntity.Empty();

            stock.isTownResources = true;
            stock.AddTownGold(0);
            stock.AddTownStone(0);
            stock.AddTownCrystal(0);
            stock.AddTownWood(0);
            stock.AddTownKnowledge(0);
        }
    }
}
