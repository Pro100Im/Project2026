using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Effect.Systems
{
    public class EffectCheckRequestCleanupSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _requests;

        private readonly List<GameEntity> _requestsBuffer = new(124);

        public EffectCheckRequestCleanupSystem()
        {
            _requests = Contexts.sharedInstance.game.GetGroup(GameMatcher.EffectCheckRequest);
        }

        public void Execute()
        {
            var requests = _requests.GetEntities(_requestsBuffer);

            for (var i = 0; i < requests.Count; i++)
                requests[i].isDestructed = true;
        }
    }
}
