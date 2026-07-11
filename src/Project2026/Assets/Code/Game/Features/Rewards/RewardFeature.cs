using Code.Game.Features.Rewards.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Rewards
{
    public class RewardFeature : Feature
    {
        public RewardFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<KillRewardSystem>());
        }
    }
}