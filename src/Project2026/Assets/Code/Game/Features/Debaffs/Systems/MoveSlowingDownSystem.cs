using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Debaffs.Systems
{
    public class MoveSlowingDownSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _entities;
        private readonly IGroup<GameEntity> _effects;

        private readonly List<GameEntity> _entitiesBuffer = new(512);
        private readonly List<GameEntity> _effectsBuffer = new(256);

        public MoveSlowingDownSystem(GameContext gameContext)
        {
            _effects = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Id,
                    GameMatcher.MoveSlowingDown,
                    GameMatcher.Effect));

            _entities = gameContext.GetGroup(GameMatcher
               .AllOf(
                   GameMatcher.Id,
                   GameMatcher.MoveSlowingDown,
                   GameMatcher.MovementSpeedBonus));
        }

        public void Execute()
        {
            var effects = _effects.GetEntities(_effectsBuffer);
            var entities = _entities.GetEntities(_entitiesBuffer);

            // To do stacking of slowing down effects.
            for (var i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                var targetEntity = GetGameEntityById.Get(effect.targetId.Value);

                if (!targetEntity.isDead && targetEntity.hasMovementSpeedBonus)
                {
                    if(targetEntity.hasMoveSlowingDown)
                    {
                        if (targetEntity.moveSlowingDown.Value < effect.moveSlowingDown.Value)
                            continue;

                        targetEntity.ReplaceMoveSlowingDown(effect.moveSlowingDown.Value);
                    }
                    else
                    {
                        targetEntity.AddMoveSlowingDown(effect.moveSlowingDown.Value);
                    }
                }
            }

            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];

                var newSpeedBonus = entity.movementSpeedBonus.Value - entity.moveSlowingDown.Value;

                if (newSpeedBonus < 0)
                    newSpeedBonus = 0;

                entity.ReplaceMovementSpeedBonus(newSpeedBonus);
                entity.RemoveMoveSlowingDown();
            }
        }
    }
}