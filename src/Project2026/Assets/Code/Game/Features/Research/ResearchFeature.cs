using Code.Game.Features.Research.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Research
{
    public class GameResearchFeature : Feature
    {
        public GameResearchFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<GameResearchSpawnSystem>());
            Add(systemFactory.Create<GameResearchAggregateSystem>());
            Add(systemFactory.Create<ResolveUnitSlotLoadoutSystem>());
            Add(systemFactory.Create<ResolveUnitSlotSystem>());
        }
    }

    public class ResearchGrantFeature : Feature
    {
        public ResearchGrantFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<GrantFireDamageMultiplierSystem>());
            Add(systemFactory.Create<GrantResistanceBonusSystem>());
            Add(systemFactory.Create<GrantKnightBlockSystem>());
        }
    }
}
