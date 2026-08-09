using Code.Game.Common.Entity;
using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Target.Systems
{
    public class SelectTargetCellSystem : IExecuteSystem
    {
        // TO DO
        private const float BlockedIdealPushPenalty = 20f;
        private const float BlockedByAttackingAllyPenalty = 100f;
        private const float BlockedByIdleAllyPenalty = 50f;
        private const float BlockedByMovingAllyPenalty = 5f;
        private const float IdealStepBonus = 20f;
        private const float SameDirectionBonus = 8f;
        private const float ReverseDirectionPenalty = 100f;
        private const float JitterPerIdMod = 0.1f;
        private const float DirectSameDirectionBonus = 0.5f;
        private const float DirectReverseDirectionPenalty = 10f;
        private const float DirectJitterPerIdMod = 0.01f;

        private readonly TargetService _targetService;

        private readonly IGroup<GameEntity> _units;
        private readonly IGroup<GameEntity> _maps;

        private readonly List<GameEntity> _buffer = new(128);

        public SelectTargetCellSystem(TargetService targetService)
        {
            var gameContext = Contexts.sharedInstance.game;

            _targetService = targetService;

            _units = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Transform,
                    GameMatcher.CurrentCell,
                    GameMatcher.Id,
                    GameMatcher.UnitSize,
                    GameMatcher.Team,
                    GameMatcher.TargetCellRequest,
                    GameMatcher.MovementAvailable,
                    GameMatcher.GridMovement));

            _maps = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.FlowFields,
                    GameMatcher.IntegrationFields,
                    GameMatcher.DefenseFlowFields,
                    GameMatcher.DefenseIntegrationFields,
                    GameMatcher.OccupField,
                    GameMatcher.ReservedField,
                    GameMatcher.SurroundField,
                    GameMatcher.TilemapMovement));
        }

        public void Execute()
        {
            var mapEntity = _maps.GetSingleEntity();

            if (mapEntity == null)
                return;

            var units = _units.GetEntities(_buffer);

            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                var cell = unit.currentCell.Value;
                var size = unit.unitSize.Value;
                var unitId = unit.id.Value;
                var myTeam = unit.team.Value;

                if (unit.hasSurroundSlot)
                {
                    SelectDirectCellToward(unit, cell, size, unitId, mapEntity, unit.surroundSlot.Value);
                    continue;
                }

                if (unit.hasDefensePatrolCell)
                {
                    SelectDirectCellToward(unit, cell, size, unitId, mapEntity, unit.defensePatrolCell.Value);
                    continue;
                }

                var useDefenseFlow = myTeam == Team.Player;
                var allFlows = useDefenseFlow ? mapEntity.defenseFlowFields.Value : mapEntity.flowFields.Value;
                var allIntegrations = useDefenseFlow ? mapEntity.defenseIntegrationFields.Value : mapEntity.integrationFields.Value;

                if (!allIntegrations.TryGetValue(size, out var integration) || !allFlows.TryGetValue(size, out var flow))
                    continue;

                if (!integration.TryGetValue(cell, out var currentCost) || currentCost == 0)
                {
                    ClearTargetCell(unit, size, unitId, mapEntity);
                    unit.isTargetCellRequest = false;

                    continue;
                }

                if (!flow.TryGetValue(cell, out var idealDir) || idealDir == Vector3Int.zero)
                {
                    ClearTargetCell(unit, size, unitId, mapEntity);
                    unit.isTargetCellRequest = false;

                    continue;
                }

                var idealStep = cell + idealDir;
                var bestCost = (float)currentCost;

                if (!CanFit(idealStep, size, unitId, mapEntity, out int blockingId))
                {
                    var pushPenalty = BlockedIdealPushPenalty;

                    if (blockingId != -1)
                    {
                        var blockingUnit = GetGameEntityById.Get(blockingId);

                        if (blockingUnit != null && blockingUnit.hasTeam && blockingUnit.team.Value == myTeam && !blockingUnit.isDead)
                        {
                            if (blockingUnit.isAttacking)
                                pushPenalty = BlockedByAttackingAllyPenalty;
                            else if (!blockingUnit.isMoving)
                                pushPenalty = BlockedByIdleAllyPenalty;
                            else
                                pushPenalty = BlockedByMovingAllyPenalty;
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

                var neighbors = _targetService.GetNeighbors(cell);

                for (var j = 0; j < neighbors.Count; j++)
                {
                    var cand = neighbors[j];

                    if (!CanFit(cand, size, unitId, mapEntity, out _))
                        continue;

                    if (IsCuttingCorner(cell, cand, size, unitId, mapEntity))
                        continue;

                    if (integration.TryGetValue(cand, out var candCost))
                    {
                        var totalCandCost = (float)candCost;
                        var moveDir = cand - cell;

                        if (cand == idealStep)
                            totalCandCost -= IdealStepBonus;

                        if (unit.hasLastDirection)
                        {
                            if (unit.lastDirection.Value == moveDir)
                                totalCandCost -= SameDirectionBonus;
                            else if (unit.lastDirection.Value == -moveDir)
                                totalCandCost += ReverseDirectionPenalty;
                        }

                        var jitter = (unitId % 10) * JitterPerIdMod;

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
                    SetTargetCell(unit, chosen, size, unitId, mapEntity);
                else
                    SetTargetCell(unit, cell, size, unitId, mapEntity);

                unit.isTargetCellRequest = false;
            }
        }

        private void SelectDirectCellToward(GameEntity unit, Vector3Int cell, Vector2Int size, int unitId, GameEntity mapEntity, Vector3Int targetCell)
        {
            if (cell == targetCell)
            {
                SetTargetCell(unit, cell, size, unitId, mapEntity);
                unit.isTargetCellRequest = false;

                return;
            }

            var bestDist = ChebyshevDistance(cell, targetCell);
            var bestCost = float.MaxValue;
            var chosen = cell;
            var found = false;
            var neighbors = _targetService.GetNeighbors(cell);

            for (var j = 0; j < neighbors.Count; j++)
            {
                var cand = neighbors[j];

                if (!CanFit(cand, size, unitId, mapEntity, out _))
                    continue;

                if (IsCuttingCorner(cell, cand, size, unitId, mapEntity))
                    continue;

                var candDist = ChebyshevDistance(cand, targetCell);

                if (candDist > bestDist)
                    continue;

                var moveDir = cand - cell;
                var totalCost = (float)candDist;

                if (unit.hasLastDirection)
                {
                    if (unit.lastDirection.Value == moveDir)
                        totalCost -= DirectSameDirectionBonus;
                    else if (unit.lastDirection.Value == -moveDir)
                        totalCost += DirectReverseDirectionPenalty;
                }

                totalCost += (unitId % 10) * DirectJitterPerIdMod;

                if (candDist < bestDist || (candDist == bestDist && totalCost < bestCost))
                {
                    bestDist = candDist;
                    bestCost = totalCost;
                    chosen = cand;
                    found = true;
                }
            }

            if (found && chosen != cell)
            {
                SetTargetCell(unit, chosen, size, unitId, mapEntity);
            }
            else
            {
                SetTargetCell(unit, cell, size, unitId, mapEntity);

                if (unit.isRangeAttack)
                    TrySettleSurroundSlotOnStuck(unit, cell, mapEntity);
                else
                    TryReleaseSurroundSlotIfUnreachable(unit, mapEntity);
            }

            unit.isTargetCellRequest = false;
        }

        private void TrySettleSurroundSlotOnStuck(GameEntity unit, Vector3Int cell, GameEntity mapEntity)
        {
            if (!unit.hasSurroundSlot
                || !unit.hasSurroundTargetId
                || !unit.hasRange)
                return;

            if (unit.surroundSlot.Value == cell)
                return;

            var target = GetGameEntityById.Get(unit.surroundTargetId.Value);

            if (target == null || target.isDead || !target.hasCurrentCell)
                return;

            if (!TargetService.IsOnAttackRing(unit, target))
                return;

            var surroundField = mapEntity.surroundField.Value;
            var unitId = unit.id.Value;
            var oldSlot = unit.surroundSlot.Value;

            if (surroundField.TryGetValue(oldSlot, out var ownerId) && ownerId == unitId)
                surroundField.Remove(oldSlot);

            surroundField[cell] = unitId;
            unit.ReplaceSurroundSlot(cell);
        }

        private void TryReleaseSurroundSlotIfUnreachable(GameEntity unit, GameEntity mapEntity)
        {
            if (!unit.hasSurroundSlot || !unit.hasSurroundTargetId || !unit.hasRange)
                return;

            var target = GetGameEntityById.Get(unit.surroundTargetId.Value);

            if (target == null || target.isDead || !target.hasCurrentCell)
                return;

            if (TargetService.IsOnAttackRing(unit, target))
                return;

            var surroundField = mapEntity.surroundField.Value;
            var unitId = unit.id.Value;
            var slot = unit.surroundSlot.Value;

            if (surroundField.TryGetValue(slot, out var ownerId) && ownerId == unitId)
                surroundField.Remove(slot);

            unit.RemoveSurroundSlot();
            unit.RemoveSurroundTargetId();
        }

        private void SetTargetCell(GameEntity unit, Vector3Int targetCell, Vector2Int size, int unitId, GameEntity mapEntity)
        {
            var reservedField = mapEntity.reservedField.Value;

            if (unit.hasTargetCell)
                ClearReservedFootprint(reservedField, unit.targetCell.Value, size, unitId);

            unit.ReplaceTargetCell(targetCell);
            WriteReservedFootprint(reservedField, targetCell, size, unitId);
        }

        private void ClearTargetCell(GameEntity unit, Vector2Int size, int unitId, GameEntity mapEntity)
        {
            if (!unit.hasTargetCell)
                return;

            ClearReservedFootprint(mapEntity.reservedField.Value, unit.targetCell.Value, size, unitId);
            unit.RemoveTargetCell();
        }

        private void ClearReservedFootprint(Dictionary<Vector3Int, int> reservedField, Vector3Int origin, Vector2Int size, int unitId)
        {
            for (var x = 0; x < size.x; x++)
            {
                for (var y = 0; y < size.y; y++)
                {
                    var cell = new Vector3Int(origin.x + x, origin.y + y, origin.z);

                    if (reservedField.TryGetValue(cell, out var ownerId) && ownerId == unitId)
                        reservedField.Remove(cell);
                }
            }
        }

        private void WriteReservedFootprint(Dictionary<Vector3Int, int> reservedField, Vector3Int origin, Vector2Int size, int unitId)
        {
            for (var x = 0; x < size.x; x++)
            {
                for (var y = 0; y < size.y; y++)
                {
                    var cell = new Vector3Int(origin.x + x, origin.y + y, origin.z);

                    reservedField[cell] = unitId;
                }
            }
        }

        private int ChebyshevDistance(Vector3Int a, Vector3Int b)
        {
            return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
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