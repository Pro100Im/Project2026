using Code.Game.Features.Damage.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Damage
{
    public class DamageFeature : Feature
    {
        public DamageFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<PhysicalDamageCanculateSystem>());
            Add(systemFactory.Create<PhysicalDamageHitEffectSystem>());

            Add(systemFactory.Create<FrostDamageCanculateSystem>());
            Add(systemFactory.Create<FrostDamageHitEffectSystem>());

            Add(systemFactory.Create<FireDamageCanculateSystem>());
            Add(systemFactory.Create<FireDamageHitEffectSystem>());

            Add(systemFactory.Create<ApplyDamageSystem>());
        }
    }
}