using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Effect.Systems
{
    public class CombustionEffectEndSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _entities;

        private readonly List<GameEntity> _entitiesBuffer = new(64);

        public CombustionEffectEndSystem(GameContext game) =>
          _entities = game.GetGroup(GameMatcher
              .AllOf(
                GameMatcher.TargetId,
                GameMatcher.Duration,
                GameMatcher.CombustionDuration,
                GameMatcher.Effect));

        public void Execute()
        {
            var entities = _entities.GetEntities(_entitiesBuffer);

            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                var targetEntity = GetGameEntityById.Get(entity.targetId.Value);

                if (targetEntity == null || targetEntity.isDead)
                    entity.isDestructed = true;

                if (entity.duration.Value > 0)
                    continue;

                if(targetEntity != null)
                    targetEntity.isCombustionDebuff = false;

                entity.isDestructed = true;
            }
        }
    }
}