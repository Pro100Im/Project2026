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
        private readonly IGroup<GameEntity> _movers;

        public PathfindingSystem(GameContext gameContext, TargetService targetService)
        {
            _targetService = targetService;

            _maps = gameContext.GetGroup(GameMatcher.AllOf(
                GameMatcher.TilemapMovement,
                GameMatcher.OccupancyMap,
                GameMatcher.GridSize));

            _movers = gameContext.GetGroup(GameMatcher.AllOf(
                GameMatcher.MovementAvailable,
                GameMatcher.CurrentCell,
                GameMatcher.UnitSize,
                GameMatcher.TargetId,
                GameMatcher.Id));
        }

        public void Execute()
        {
            var mapEntity = _maps.GetSingleEntity();
            if (mapEntity == null) return;

            var occupancyMap = mapEntity.occupancyMap.Value;
            var tilemapMovement = mapEntity.tilemapMovement.Value;
            var gridSize = mapEntity.gridSize.Value;

            foreach (var entity in _movers)
            {

                //var targetEntity = GetGameEntityById.Get(entity.targetId.Value);
                //if (targetEntity == null || !targetEntity.hasCurrentCell)
                //{
                //    if (entity.hasPath) entity.RemovePath();
                //    continue;
                //}

                Vector3Int targetPos = new Vector3Int(6, 2);/*targetEntity.currentCell.Value*/;

                if (ShouldRecalculatePath(entity, targetPos, occupancyMap, gridSize))
                {
                    var pathCells = _targetService.FindPath(
                        entity.id.Value,
                        entity.currentCell.Value,
                        targetPos,
                        tilemapMovement,
                        occupancyMap,
                        entity.unitSize.Value
                    );

                    if (pathCells != null && pathCells.Count > 0)
                    {

                        var newPath = new Queue<Vector3Int>(pathCells);
                        entity.ReplacePath(newPath);
                    }
                    else
                    {
                        if (!IsAdjacent(entity.currentCell.Value, targetPos, entity.unitSize.Value))
                        {
                            if (entity.hasPath) entity.RemovePath();
                        }
                    }
                }
            }
        }

        private bool ShouldRecalculatePath(GameEntity entity, Vector3Int targetPos, Dictionary<Vector3Int, int> occupancy, Vector2 gridSize)
        {
            if (!entity.hasPath || entity.path.Value.Count == 0) return true;

            Vector3Int lastPathCell = entity.path.Value.Last();
            if (Vector3Int.Distance(lastPathCell, targetPos) > 1.5f) return true;

            Vector3Int nextCell = entity.path.Value.Peek();
            if (occupancy.TryGetValue(nextCell, out var ownerId))
            {
                if (ownerId != entity.id.Value) return true;
            }

            return false;
        }

        private bool IsAdjacent(Vector3Int current, Vector3Int target, Vector2Int size)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    var part = new Vector3Int(current.x + x, current.y + y, 0);
                    if (Vector3Int.Distance(part, target) <= 1.5f) return true;
                }
            }
            return false;
        }
    }
}