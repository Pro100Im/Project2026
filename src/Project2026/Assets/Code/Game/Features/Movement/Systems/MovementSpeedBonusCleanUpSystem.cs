using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Movement.Systems
{
    public class MovementSpeedBonusCleanUpSystem : ICleanupSystem
    {
        private readonly IGroup<GameEntity> _entities;

        private readonly List<GameEntity> _entitiesBuffer = new(512);

        public MovementSpeedBonusCleanUpSystem(GameContext game) =>
          _entities = game.GetGroup(GameMatcher
              .AllOf(
                GameMatcher.Id,
                GameMatcher.MovementSpeedBonus));

        public void Cleanup()
        {
            var entities = _entities.GetEntities(_entitiesBuffer);

            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];

                entity.ReplaceMovementSpeedBonus(1);
            }
        }
    }
}