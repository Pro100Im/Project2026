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

            foreach (var entity in _movers)
            {
                // Пока таргет вручную, но логика та же
                Vector3Int targetPos = new Vector3Int(6, 2);

                // ПРАВИЛО 1: Если мы уже стоим на клетке, которая "бьет" цель, 
                // или мы часть большого юнита, который стоит рядом - СТОИМ.
                if (IsAdjacent(entity.currentCell.Value, targetPos, entity.unitSize.Value))
                {
                    if (entity.hasPath) entity.RemovePath();
                    entity.isMoving = false;
                    continue;
                }

                // ПРАВИЛО 2: Пересчитываем путь только если:
                // - Пути нет
                // - Следующая клетка пути занята КЕМ-ТО ДРУГИМ
                if (ShouldRecalculatePath(entity, targetPos, occupancyMap))
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
                        entity.ReplacePath(new Queue<Vector3Int>(pathCells));
                    }
                    else
                    {
                        // Если путь не найден (все занято вокруг цели)
                        // Юнит должен просто стоять и ждать, а не дергаться
                        entity.isMoving = false;
                    }
                }
            }
        }

        private bool ShouldRecalculatePath(GameEntity entity, Vector3Int targetPos, Dictionary<Vector3Int, int> occupancy)
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
            // Проверяем все клетки, которые занимает юнит
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector3Int partOfUnit = new Vector3Int(current.x + x, current.y + y, 0);

                    // Дистанция 1.0 означает, что клетки стоят вплотную (по горизонтали/вертикали)
                    // 1.42f включает диагонали.
                    if (Vector3Int.Distance(partOfUnit, target) <= 1.5f)
                        return true;
                }
            }
            return false;
        }
    }
}