using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Effect.Systems
{
    public class ChillEffectEndSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _entities;

        private readonly List<GameEntity> _entitiesBuffer = new(64);

        public ChillEffectEndSystem() =>
          _entities = Contexts.sharedInstance.game.GetGroup(GameMatcher
              .AllOf(
                GameMatcher.TargetId,
                GameMatcher.Duration,
                GameMatcher.ChillDuration,
                GameMatcher.Effect));

        public void Execute()
        {
            var entities = _entities.GetEntities(_entitiesBuffer);

            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                var targetEntity = GetGameEntityById.Get(entity.targetId.Value);

                if(targetEntity == null || targetEntity.isDead)
                    entity.isDestructed = true;

                if (entity.duration.Value > 0)
                    continue;

                if (targetEntity != null)
                    targetEntity.isChillDebuff = false;

                entity.isDestructed = true;
            }
        }
    }
}