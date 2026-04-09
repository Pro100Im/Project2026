using Code.Common.Entity;
using Code.Common.Time;
using Entitas;

namespace Code.Common.Destruct.Systems
{
    public class DelayDestructSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _entities;

        public DelayDestructSystem(GameContext game, ITimeService time)
        {
            _entities = game.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.TargetId,
                GameMatcher.DelayDestruct,
                GameMatcher.Duration
                ));
        }

        public void Execute()
        {
            foreach (GameEntity entity in _entities)
            {
                if(entity.duration.Value <= 0)
                {
                    var targetEntity = GetGameEntityById.Get(entity.targetId.Value);

                    targetEntity.isDestructed = true;
                    entity.isDestructed = true;
                }
            }
        }
    }
}