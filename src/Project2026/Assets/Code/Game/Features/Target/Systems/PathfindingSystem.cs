using Entitas;

namespace Code.Game.Features.Target.Systems
{
    public class PathfindingSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _maps;
        private readonly IGroup<GameEntity> _enemies;
        private readonly IGroup<GameEntity> _targets;

        public PathfindingSystem(GameContext gameContext)
        {
            _maps = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.TilemapMovement,
                GameMatcher.OccupancyMap,
                GameMatcher.GridSize));

            _enemies = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.MovementAvailable,
                GameMatcher.UnitSize,
                GameMatcher.Enemy));

            _targets = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.Targetable,
                GameMatcher.UnitSize,
                GameMatcher.Player));
        }

        public void Execute()
        {
            foreach (var map in _maps)
            {
                var gridSize = map.gridSize.Value;

                foreach (var enemy in _enemies)
                {
                    if (!enemy.hasTargetId) 
                        continue;


                }
            }
        }
    }
}