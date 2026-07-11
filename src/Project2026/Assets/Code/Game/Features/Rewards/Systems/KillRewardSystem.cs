using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Rewards.Systems
{
    public class KillRewardSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _rewards;

        private readonly List<GameEntity> _rewardsBuffer = new(86);

        public KillRewardSystem(GameContext gameContext)
        {
            _rewards = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Id,
                    GameMatcher.Reward,
                    GameMatcher.Dead
                    )
                .NoneOf(GameMatcher.Destructed));
        }

        public void Execute()
        {
            var rewards = _rewards.GetEntities(_rewardsBuffer);

            for (var i = 0; i < rewards.Count; i++)
            {
                var reward = rewards[i];
                var entity = CreateGameEntity.Empty();
                var spawnPos = reward.hasSpawnPosition ? reward.spawnPosition.Value : reward.woldPos.Value;

                entity.AddSpawnPosition(spawnPos);

                for (var j = 0; j < reward.reward.Value.Properties.Length; j++)
                {
                    var property = reward.reward.Value.Properties[j];

                    property.Apply(entity);
                }

                reward.RemoveReward();
            }
        }
    }
}