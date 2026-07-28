using Code.Game.Features.Effect.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Effect
{
    public class EffectFeature : Feature
    {
        public EffectFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<ApplyChillEffectSystem>());
            Add(systemFactory.Create<ApplyCombustionEffectSystem>());
            Add(systemFactory.Create<ApplyFreezeEffectSystem>());
            Add(systemFactory.Create<EffectCheckRequestCleanupSystem>());

            Add(systemFactory.Create<ChillEffectEndSystem>());
            Add(systemFactory.Create<CombustionEffectEndSystem>());
            Add(systemFactory.Create<FreezeEffectEndSystem>());
        }
    }
}
