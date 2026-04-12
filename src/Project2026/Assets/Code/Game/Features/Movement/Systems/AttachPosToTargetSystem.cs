using Code.Game.Common.Entity;
using Entitas;

namespace Code.Game.Features.Movement.Systems
{
    public class AttachPosToTargetSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _followers;

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
            foreach (var follower in _followers)
            {
                var targetEntity = GetGameEntityById.Get(follower.targetId.Value);

                if (targetEntity.isMoving)
                {
                    follower.transform.Value.position = targetEntity.transform.Value.position + follower.movementOffset.Value;
                }
            }
        }
    }
}