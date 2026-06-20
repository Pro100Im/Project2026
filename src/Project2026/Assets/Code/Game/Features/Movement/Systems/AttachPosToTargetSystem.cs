 using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Movement.Systems
{
    public class AttachPosToTargetSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _followers;

        private readonly List<GameEntity> _followersBuffer = new(512);

        public AttachPosToTargetSystem(GameContext gameContext)
        { 
            _followers = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Attached,
                    GameMatcher.TargetId,
                    GameMatcher.Transform));
        }

        public void Execute()
        {
            var entities = _followers.GetEntities(_followersBuffer);

            for (var i = 0; i < entities.Count; i++)
            {
                var follower = entities[i];
                var targetEntity = GetGameEntityById.Get(follower.targetId.Value);

                if (targetEntity.isMoving)
                    follower.transform.Value.position = targetEntity.woldPos.Value + follower.movementOffset.Value;
            }
        }
    }
}