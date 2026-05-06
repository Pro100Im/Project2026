using Code.Game.Common.Entity;
using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Target.Systems
{
    // to do remove magic numbs
    public class SelectTargetCellSystem : IExecuteSystem
    {
        private readonly TargetService _targetService;

        private readonly IGroup<GameEntity> _units;
        private readonly IGroup<GameEntity> _maps;

        private readonly List<GameEntity> _buffer = new(128);

        public SelectTargetCellSystem(GameContext context, TargetService targetService)
        {
            _targetService = targetService;

            _units = context.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Transform,
                    GameMatcher.CurrentCell,
                    GameMatcher.Id,
                    GameMatcher.UnitSize,
                    GameMatcher.Team)
                .NoneOf(GameMatcher.Moving));

            _maps = context.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.FlowFields,
                    GameMatcher.IntegrationFields,
                    GameMatcher.OccupField,
                    GameMatcher.ReservedField,
                    GameMatcher.TilemapMovement));
        }

        public void Execute()
        {
            var mapEntity = _maps.GetSingleEntity();

            if (mapEntity == null)
                return;

            var allFlows = mapEntity.flowFields.Value;
            var allIntegrations = mapEntity.integrationFields.Value;

            foreach (var unit in _units.GetEntities(_buffer))
            {
                var cell = unit.currentCell.Value;
                var size = unit.unitSize.Value;
                var unitId = unit.id.Value;
                var myTeam = unit.team.Value;

                if (!allIntegrations.TryGetValue(size, out var integration) || !allFlows.TryGetValue(size, out var flow))
                    continue;

                if (!integration.TryGetValue(cell, out var currentCost) || currentCost == 0)
                {
                    if (unit.hasTargetCell)
                        unit.RemoveTargetCell();

                    continue;
                }

                if (!flow.TryGetValue(cell, out var idealDir) || idealDir == Vector3Int.zero)
                {
                    if (unit.hasTargetCell)
                        unit.RemoveTargetCell();

                    continue;
                }

                var idealStep = cell + idealDir;
                var bestCost = (float)currentCost;

                if (!CanFit(idealStep, size, unitId, mapEntity, out int blockingId))
                {
                    int pushPenalty = 20;

                    if (blockingId != -1)
                    {
                        var blockingUnit = GetGameEntityById.Get(blockingId);

                        if (blockingUnit != null && blockingUnit.hasTeam && blockingUnit.team.Value == myTeam && !blockingUnit.isDead)
                        {
                            if (blockingUnit.isAttacking)
                                pushPenalty = 100;
                            else if (!blockingUnit.isMoving)
                                pushPenalty = 50;
                            else
                                pushPenalty = 5;
                        }
                    }

                    bestCost += pushPenalty;
                }

                var chosen = cell;
                var found = false;

                if (CanFit(idealStep, size, unitId, mapEntity, out _))
                {
                    chosen = idealStep;
                    found = true;
                    bestCost = integration[idealStep];
                }

                foreach (var cand in _targetService.GetNeighbors(cell))
                {
                    if (!CanFit(cand, size, unitId, mapEntity, out _))
                        continue;

                    if (IsCuttingCorner(cell, cand, size, unitId, mapEntity))
                        continue;

                    if (integration.TryGetValue(cand, out var candCost))
                    {
                        var totalCandCost = (float)candCost;
                        var moveDir = cand - cell;

                        if (cand == idealStep)
                            totalCandCost -= 20.0f;

                        if (unit.hasLastDirection)
                        {
                            if (unit.lastDirection.Value == moveDir)
                                totalCandCost -= 8.0f;
                            else if (unit.lastDirection.Value == -moveDir)
                                totalCandCost += 100.0f;
                        }

                        var jitter = (unitId % 10) * 0.1f;
                        totalCandCost += jitter;

                        if (totalCandCost < bestCost)
                        {
                            bestCost = totalCandCost;
                            chosen = cand;
                            found = true;
                        }
                    }
                }

                if (found && chosen != cell)
                    unit.ReplaceTargetCell(chosen);
                else if (unit.hasTargetCell)
                    unit.RemoveTargetCell();
            }
        }

        private bool CanFit(Vector3Int origin, Vector2Int size, int unitId, GameEntity map, out int blockingEntityId)
        {
            blockingEntityId = -1;

            var tilemap = map.tilemapMovement.Value;
            var occupField = map.occupField.Value;
            var reservedField = map.reservedField.Value;

            for (var x = 0; x < size.x; x++)
            {
                for (var y = 0; y < size.y; y++)
                {
                    var checkPos = new Vector3Int(origin.x + x, origin.y + y, 0);

                    if (!tilemap.ContainsKey(checkPos))
                        return false;

                    if (occupField.TryGetValue(checkPos, out var occId) && occId != unitId)
                    {
                        blockingEntityId = occId;

                        return false;
                    }

                    if (reservedField.TryGetValue(checkPos, out var resId) && resId != unitId)
                    {
                        blockingEntityId = resId;

                        return false;
                    }
                }
            }

            return true;
        }

        private bool IsCuttingCorner(Vector3Int current, Vector3Int neighbor, Vector2Int size, int unitId, GameEntity map)
        {
            if (current.x != neighbor.x && current.y != neighbor.y)
            {
                var corner1 = new Vector3Int(neighbor.x, current.y, 0);
                var corner2 = new Vector3Int(current.x, neighbor.y, 0);

                if (!CanFit(corner1, size, unitId, map, out _) || !CanFit(corner2, size, unitId, map, out _))
                    return true;
            }

            return false;
        }
    }
}