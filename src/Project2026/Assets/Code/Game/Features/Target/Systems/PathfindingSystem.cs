using Code.Game.Common.Entity;
using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Game.Features.Target.Systems
{
    public class PathfindingSystem : IExecuteSystem
    {
        private readonly TargetService _targetService;

        private readonly IGroup<GameEntity> _maps;
        private readonly IGroup<GameEntity> _enemies;

        public PathfindingSystem(GameContext gameContext, TargetService targetService)
        {
            _targetService = targetService;

            _maps = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.TilemapMovement,
                GameMatcher.OccupancyMap,
                GameMatcher.GridSize));

            _enemies = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.MovementAvailable,
                GameMatcher.CurrentCell,
                GameMatcher.UnitSize,
                GameMatcher.TargetId,
                GameMatcher.Enemy));
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

                    var target = GetGameEntityById.Get(enemy.targetId.Value);
                    var targetPos = new Vector3Int(6, 1);/*target.transform.Value.positio*/;

                    if (!enemy.hasPath || enemy.path.Value.Count == 0 || Vector3.Distance(enemy.path.Value.Last(), targetPos) > gridSize.x
                        || map.occupancyMap.Value.ContainsKey(enemy.path.Value.Peek()))
                    {
                        var pathCells = _targetService.AStar(enemy.currentCell.Value, targetPos, map.tilemapMovement.Value,
                            map.occupancyMap.Value, enemy.unitSize.Value);
                        var newPath = new Queue<Vector3Int>(pathCells);

                        if (!enemy.hasPath)
                            enemy.AddPath(newPath);
                        else if(newPath.Count != 0)
                            enemy.ReplacePath(newPath);
                    }
                }
            }
        }
    }
}