using Code.Game.Common.Time;
using Entitas;
using System.Linq;
using UnityEngine;

namespace Code.Game.Features.Movement.Systems
{
    public class EnemiesMovementSystem : IExecuteSystem
    {
        private readonly ITimeService _timeService;

        private readonly IGroup<GameEntity> _maps;
        private readonly IGroup<GameEntity> _enemies;

        public EnemiesMovementSystem(GameContext gameContext, ITimeService timeService)
        {
            _timeService = timeService;

            _maps = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.TilemapMovement,
                GameMatcher.OccupancyMap,
                GameMatcher.GridSize));

            _enemies = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.MovementAvailable,
                GameMatcher.MovementSpeed,
                GameMatcher.Path,
                GameMatcher.Enemy));
        }

        public void Execute()
        {
            foreach (var map in _maps)
            {
                foreach (var enemy in _enemies)
                {
                    if (enemy.path.Value.Count == 0)
                    {
                        enemy.isMoving = false;
                        continue;
                    }

                    var nextCellIndex = enemy.path.Value.Peek();
                    var nextCellWorld = map.tilemapMovement.Value[nextCellIndex];

                    var currentPos = enemy.transform.Value.position;
                    var dir = (nextCellWorld - currentPos).normalized;
                    var speed = enemy.movementSpeed.Value;
                    var newPos = currentPos + dir * speed * _timeService.DeltaTime;

                    if (Vector3.Distance(currentPos, nextCellWorld) < 0.1f)
                    {
                        enemy.path.Value.Dequeue();
                        enemy.ReplaceCurrentCell(nextCellIndex);

                        foreach (var kvp in map.occupancyMap.Value.Where(kvp => kvp.Value == enemy.id.Value).ToList())
                            map.occupancyMap.Value.Remove(kvp.Key);

                        for (int x = 0; x < enemy.unitSize.Value.x; x++)
                        {
                            for (int y = 0; y < enemy.unitSize.Value.y; y++)
                            {
                                var cellIndex = new Vector3Int(nextCellIndex.x + x, nextCellIndex.y + y, 0);
                                map.occupancyMap.Value[cellIndex] = enemy.id.Value;
                            }
                        }

                        if (enemy.path.Value.Count == 0)
                        {
                            enemy.isMoving = false;
                        }
                    }

                    enemy.isMoving = true;
                    enemy.transform.Value.position = newPos;
                }
            }
        }
    }
}