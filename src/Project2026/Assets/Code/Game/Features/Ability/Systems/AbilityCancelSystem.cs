using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Ability.Systems
{
    public class AbilityCancelSystem : IExecuteSystem
    {
        private readonly IGroup<InputEntity> _cancelIntents;
        private readonly IGroup<GameEntity> _cancelRequests;
        private readonly IGroup<GameEntity> _targeting;

        private readonly List<InputEntity> _cancelBuffer = new(4);
        private readonly List<GameEntity> _cancelRequestsBuffer = new(4);
        private readonly List<GameEntity> _targetingBuffer = new(2);

        public AbilityCancelSystem(InputContext inputContext, GameContext gameContext)
        {
            _cancelIntents = inputContext.GetGroup(InputMatcher
                .AllOf(InputMatcher.CancelIntent)
                .NoneOf(InputMatcher.Destructed));

            _cancelRequests = gameContext.GetGroup(GameMatcher
                .AllOf(GameMatcher.AbilityCancelRequest)
                .NoneOf(GameMatcher.Destructed));

            _targeting = gameContext.GetGroup(GameMatcher
                .AllOf(GameMatcher.AbilityTargeting)
                .NoneOf(GameMatcher.Destructed));
        }

        public void Execute()
        {
            var cancels = _cancelIntents.GetEntities(_cancelBuffer);
            var requests = _cancelRequests.GetEntities(_cancelRequestsBuffer);

            for (var i = 0; i < requests.Count; i++)
                requests[i].isDestructed = true;

            if (cancels.Count == 0 && requests.Count == 0)
                return;

            var targeting = _targeting.GetEntities(_targetingBuffer);

            for (var i = 0; i < targeting.Count; i++)
            {
                var entity = targeting[i];

                entity.isAbilityTargeting = false;
                entity.isAbilityRangeShowed = false;
                entity.isDestructed = true;
            }
        }
    }
}
