using Code.Game.Features.Attack.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Attack
{
    public class AttackFeature : Feature
    {
        public AttackFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<AttackStartSystem>());
            Add(systemFactory.Create<MeleeAttackEndSystem>());
            Add(systemFactory.Create<RangeAttackEndSystem>());
            Add(systemFactory.Create<RangeAttackHitSystem>());
            Add(systemFactory.Create<RangeAreaAttackHitSystem>());
        }
    }
}