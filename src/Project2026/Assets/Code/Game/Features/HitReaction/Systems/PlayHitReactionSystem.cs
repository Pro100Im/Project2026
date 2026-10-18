using Code.Game.Common.Time;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.HitReaction.Systems
{
    public class PlayHitReactionSystem : IExecuteSystem
    {
        private readonly ITimeService _timeService;
        private readonly IGroup<GameEntity> _requests;
        private readonly IGroup<GameEntity> _reactions;

        private readonly List<GameEntity> _requestsBuffer = new(128);
        private readonly List<GameEntity> _reactionsBuffer = new(512);

        public PlayHitReactionSystem(ITimeService timeService)
        {
            _timeService = timeService;

            var game = Contexts.sharedInstance.game;

            _requests = game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.HitReaction,
                    GameMatcher.HitReactionRequest));

            _reactions = game.GetGroup(GameMatcher.HitReaction);
        }

        public void Execute()
        {
            PlayRequests();
            TickReactions();
        }

        private void PlayRequests()
        {
            var requests = _requests.GetEntities(_requestsBuffer);

            for (var i = 0; i < requests.Count; i++)
            {
                var target = requests[i];
                var view = target.hitReaction.Value;

                if (view != null)
                    view.Play(target.hitReactionRequest.Value);

                target.RemoveHitReactionRequest();
            }
        }

        private void TickReactions()
        {
            var reactions = _reactions.GetEntities(_reactionsBuffer);
            var deltaTime = _timeService.GameDeltaTime;

            for (var i = 0; i < reactions.Count; i++)
            {
                var view = reactions[i].hitReaction.Value;

                if (view != null)
                    view.Tick(deltaTime);
            }
        }
    }
}
