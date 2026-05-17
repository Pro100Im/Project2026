using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Movement.Systems
{
    public class FlipAlongMoveDirectionSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _units;

        private readonly List<GameEntity> _unitsBuffer = new(512);

        public FlipAlongMoveDirectionSystem(GameContext context)
        {
            _units = context.GetGroup(GameMatcher.AllOf(
                GameMatcher.CurrentCell,
                GameMatcher.TargetCell,
                GameMatcher.SpriteRenderer,
                GameMatcher.Moving));
        }

        public void Execute()
        {
            var units = _units.GetEntities(_unitsBuffer);

            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                var dx = unit.targetCell.Value.x - unit.currentCell.Value.x;

                if (dx == 0) 
                    continue;

                var shouldFlipX = dx < 0;

                if (unit.spriteRenderer.Value.flipX != shouldFlipX)
                    unit.spriteRenderer.Value.flipX = shouldFlipX;
            }
        }
    }
}