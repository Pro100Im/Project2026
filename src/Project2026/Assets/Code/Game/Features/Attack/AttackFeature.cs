using Code.Game.Features.Attack.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Attack
{
    public class AttackFeature : Feature
    {
        public AttackFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<SearchingClosestTargetSystem>());
            Add(systemFactory.Create<AttackStartSystem>());
        }
    }
}