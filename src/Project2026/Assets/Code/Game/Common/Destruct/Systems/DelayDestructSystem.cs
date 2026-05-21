using Code.Game.Common.Entity;
using Code.Game.Common.Time;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Common.Destruct.Systems
{
    public class DelayDestructSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _entities;

        private readonly List<GameEntity> _entitiesBuffer = new(128);

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
            var entities = _entities.GetEntities(_entitiesBuffer);

            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];

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