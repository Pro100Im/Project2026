using Entitas;

namespace Code.Game.Features.Movement.Systems
{
    public class FlipAlongMoveDirectionSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _units;

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
            foreach (var unit in _units)
            {
                var dx = unit.targetCell.Value.x - unit.currentCell.Value.x;

                if (dx == 0) 
                    continue;

                bool shouldFlipX = dx < 0;

                if (unit.spriteRenderer.Value.flipX != shouldFlipX)
                    unit.spriteRenderer.Value.flipX = shouldFlipX;
            }
        }
    }
}