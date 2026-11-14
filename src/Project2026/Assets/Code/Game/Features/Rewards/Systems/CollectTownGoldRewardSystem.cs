using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Rewards.Systems
{
    public class CollectTownGoldRewardSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _rewards;
        private readonly IGroup<GameEntity> _sessions;

        private readonly List<GameEntity> _rewardsBuffer = new(86);
        private readonly List<GameEntity> _sessionBuffer = new(2);

        public CollectTownGoldRewardSystem()
        {
            var gameContext = Contexts.sharedInstance.game;

            _rewards = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.TownGoldReward,
                    GameMatcher.Dead)
                .NoneOf(GameMatcher.Destructed));

            _sessions = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.GameSession,
                    GameMatcher.BattleTownGold));
        }

        public void Execute()
        {
            var sessions = _sessions.GetEntities(_sessionBuffer);

            if (sessions.Count == 0)
                return;

            var rewards = _rewards.GetEntities(_rewardsBuffer);

            if (rewards.Count == 0)
                return;

            var session = sessions[0];
            var total = session.battleTownGold.Value;

            for (var i = 0; i < rewards.Count; i++)
            {
                var reward = rewards[i];
                total += reward.townGoldReward.Value;
                reward.RemoveTownGoldReward();
            }

            session.ReplaceBattleTownGold(total);
        }
    }
}
