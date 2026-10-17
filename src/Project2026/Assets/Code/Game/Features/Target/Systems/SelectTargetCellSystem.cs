using Code.Game.Common.Entity;
using Code.Game.Common.Time;
using Code.Game.Features.Movement;
using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Target.Systems
{
    public class SelectTargetCellSystem : IExecuteSystem
    {
        private const float IdealStepBonus = 20f;
        private const float SameDirectionBonus = 8f;
        private const float DirectSameDirectionBonus = 0.5f;
        private const float MoveWaitThreshold = 0.35f;
        private const int DetourCommitSteps = 2;

        private readonly ITimeService _timeService;
        private readonly TargetService _targetService;

        private readonly IGroup<GameEntity> _units;
        private readonly IGroup<GameEntity> _maps;

        private readonly List<GameEntity> _buffer = new(128);

        public SelectTargetCellSystem(ITimeService timeService, TargetService targetService)
        {
            var gameContext = Contexts.sharedInstance.game;

            _timeService = timeService;
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

            var deltaTime = _timeService.GameDeltaTime;
            var units = _units.GetEntities(_buffer);

            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                var cell = unit.currentCell.Value;
                var size = unit.unitSize.Value;
                var unitId = unit.id.Value;

                if (unit.hasSurroundSlot)
                {
                    SelectDirectCellToward(unit, cell, size, unitId, mapEntity, unit.surroundSlot.Value, deltaTime);
                    continue;
                }

                if (unit.hasDefensePatrolCell)
                {
                    SelectDirectCellToward(unit, cell, size, unitId, mapEntity, unit.defensePatrolCell.Value, deltaTime);
                    continue;
                }

                SelectFlowCell(unit, cell, size, unitId, mapEntity, deltaTime);
            }
        }

        private void SelectFlowCell(
            GameEntity unit,
            Vector3Int cell,
            Vector2Int size,
            int unitId,
            GameEntity mapEntity,
            float deltaTime)
        {
            var useDefenseFlow = unit.team.Value == Team.Player;
            var allFlows = useDefenseFlow ? mapEntity.defenseFlowFields.Value : mapEntity.flowFields.Value;
            var allIntegrations = useDefenseFlow ? mapEntity.defenseIntegrationFields.Value : mapEntity.integrationFields.Value;

            if (!allIntegrations.TryGetValue(size, out var integration) || !allFlows.TryGetValue(size, out var flow))
                return;

            if (!integration.TryGetValue(cell, out var currentCost) || currentCost == 0
                || !flow.TryGetValue(cell, out var idealDir) || idealDir == Vector3Int.zero)
            {
                ClearTargetCell(unit, size, unitId, mapEntity);
                ClearMoveWait(unit);
                ClearMoveCommit(unit);
                unit.isTargetCellRequest = false;
                return;
            }

            var progress = new StepLimits
            {
                Integration = integration,
                CapCost = true,
                MaxCost = currentCost,
                PreferIdeal = true,
                IdealStep = cell + idealDir,
                DirectionBonus = SameDirectionBonus
            };

            var detour = new StepLimits
            {
                Integration = integration,
                DirectionBonus = SameDirectionBonus
            };

            ChooseStep(unit, cell, size, unitId, mapEntity, deltaTime, false, currentCost, progress, detour);
        }

        private void SelectDirectCellToward(
            GameEntity unit,
            Vector3Int cell,
            Vector2Int size,
            int unitId,
            GameEntity mapEntity,
            Vector3Int targetCell,
            float deltaTime)
        {
            if (cell == targetCell)
            {
                Stay(unit, cell, size, unitId, mapEntity);
                ClearMoveWait(unit);
                ClearMoveCommit(unit);
                return;
            }

            var currentDist = ChebyshevDistance(cell, targetCell);

            var progress = new StepLimits
            {
                MeasureDistance = true,
                MaxDistance = currentDist - 1,
                Goal = targetCell,
                DirectionBonus = DirectSameDirectionBonus
            };

            var detour = new StepLimits
            {
                MeasureDistance = true,
                MaxDistance = currentDist,
                Goal = targetCell,
                DirectionBonus = DirectSameDirectionBonus
            };

            ChooseStep(unit, cell, size, unitId, mapEntity, deltaTime, true, currentDist, progress, detour);
        }

        private void ChooseStep(
            GameEntity unit,
            Vector3Int cell,
            Vector2Int size,
            int unitId,
            GameEntity mapEntity,
            float deltaTime,
            bool direct,
            float metric,
            StepLimits progress,
            StepLimits detour)
        {
            if (unit.hasMoveCommit && unit.moveCommit.Value.Direct != direct)
                unit.RemoveMoveCommit();

            ClearCommitIfProgress(unit, metric);

            if (unit.hasMoveCommit)
            {
                var commitLimits = detour;
                commitLimits.AlongCommit = true;

                if (TryPickStep(unit, cell, size, unitId, mapEntity, commitLimits, out var committed))
                {
                    AcceptStep(unit, committed, size, unitId, mapEntity);
                    ConsumeCommit(unit);
                    return;
                }

                if (AccumulateWait(unit, deltaTime) < MoveWaitThreshold)
                {
                    Stay(unit, cell, size, unitId, mapEntity);
                    return;
                }
            }
            else if (TryPickStep(unit, cell, size, unitId, mapEntity, progress, out var step))
            {
                AcceptStep(unit, step, size, unitId, mapEntity);
                return;
            }
            else if (AccumulateWait(unit, deltaTime) < MoveWaitThreshold)
            {
                Stay(unit, cell, size, unitId, mapEntity);
                return;
            }

            if (TryPickStep(unit, cell, size, unitId, mapEntity, detour, out var detourStep))
            {
                AcceptStep(unit, detourStep, size, unitId, mapEntity);
                SetMoveCommit(unit, detourStep - cell, metric, direct);
                return;
            }

            if (direct)
            {
                if (unit.isRangeAttack)
                    TrySettleSurroundSlotOnStuck(unit, cell, mapEntity);
                else
                    TryReleaseSurroundSlotIfUnreachable(unit, mapEntity);
            }

            Stay(unit, cell, size, unitId, mapEntity);
        }

        private bool TryPickStep(
            GameEntity unit,
            Vector3Int cell,
            Vector2Int size,
            int unitId,
            GameEntity mapEntity,
            StepLimits limits,
            out Vector3Int chosen)
        {
            chosen = cell;
            var bestScore = float.MaxValue;
            var bestDistance = int.MaxValue;
            var found = false;
            var commitDirection = limits.AlongCommit && unit.hasMoveCommit
                ? unit.moveCommit.Value.Direction
                : Vector3Int.zero;
            var neighbors = _targetService.GetNeighbors(cell);

            for (var j = 0; j < neighbors.Count; j++)
            {
                var cand = neighbors[j];
                var moveDir = cand - cell;
                var distance = 0;
                float score;

                if (limits.AlongCommit && Dot(moveDir, commitDirection) <= 0)
                    continue;

                if (limits.MeasureDistance)
                {
                    distance = ChebyshevDistance(cand, limits.Goal);

                    if (distance > limits.MaxDistance)
                        continue;

                    score = distance;
                }
                else
                {
                    if (limits.Integration == null || !limits.Integration.TryGetValue(cand, out var cost))
                        continue;

                    if (limits.CapCost && cost > limits.MaxCost)
                        continue;

                    score = cost;
                }

                if (!CanStep(cell, cand, size, unitId, mapEntity, unit))
                    continue;

                if (limits.PreferIdeal && cand == limits.IdealStep)
                    score -= IdealStepBonus;

                if (unit.hasLastDirection && unit.lastDirection.Value == moveDir)
                    score -= limits.DirectionBonus;

                var better = limits.MeasureDistance
                    ? distance < bestDistance || (distance == bestDistance && score < bestScore)
                    : score < bestScore;

                if (!better)
                    continue;

                bestScore = score;
                bestDistance = distance;
                chosen = cand;
                found = true;
            }

            return found;
        }

        private void AcceptStep(GameEntity unit, Vector3Int chosen, Vector2Int size, int unitId, GameEntity mapEntity)
        {
            SetTargetCell(unit, chosen, size, unitId, mapEntity);
            ClearMoveWait(unit);
            unit.isTargetCellRequest = false;
        }

        private void Stay(GameEntity unit, Vector3Int cell, Vector2Int size, int unitId, GameEntity mapEntity)
        {
            SetTargetCell(unit, cell, size, unitId, mapEntity);
            unit.isTargetCellRequest = false;
        }

        private float AccumulateWait(GameEntity unit, float deltaTime)
        {
            var wait = (unit.hasMoveWait ? unit.moveWait.Value : 0f) + deltaTime;

            if (unit.hasMoveWait)
                unit.ReplaceMoveWait(wait);
            else
                unit.AddMoveWait(wait);

            return wait;
        }

        private static void ClearMoveWait(GameEntity unit)
        {
            if (unit.hasMoveWait)
                unit.RemoveMoveWait();
        }

        private static void ClearCommitIfProgress(GameEntity unit, float metric)
        {
            if (unit.hasMoveCommit && metric < unit.moveCommit.Value.StartCost)
                unit.RemoveMoveCommit();
        }

        private static void ClearMoveCommit(GameEntity unit)
        {
            if (unit.hasMoveCommit)
                unit.RemoveMoveCommit();
        }

        private static void SetMoveCommit(GameEntity unit, Vector3Int direction, float startCost, bool direct)
        {
            var data = new MoveCommitData
            {
                Direction = direction,
                Steps = DetourCommitSteps - 1,
                StartCost = startCost,
                Direct = direct
            };

            if (unit.hasMoveCommit)
                unit.ReplaceMoveCommit(data);
            else
                unit.AddMoveCommit(data);
        }

        private static void ConsumeCommit(GameEntity unit)
        {
            if (!unit.hasMoveCommit)
                return;

            var commit = unit.moveCommit.Value;
            commit.Steps -= 1;

            if (commit.Steps <= 0)
                unit.RemoveMoveCommit();
            else
                unit.ReplaceMoveCommit(commit);
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

        private bool CanStep(Vector3Int current, Vector3Int neighbor, Vector2Int size, int unitId, GameEntity map, GameEntity unit)
        {
            if (!CanFit(neighbor, size, unitId, map))
                return false;

            if (IsCuttingCorner(current, neighbor, size, unitId, map))
                return false;

            var moveDir = neighbor - current;

            return !unit.hasLastDirection || unit.lastDirection.Value != -moveDir;
        }

        private int ChebyshevDistance(Vector3Int a, Vector3Int b)
        {
            return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
        }

        private static int Dot(Vector3Int a, Vector3Int b)
        {
            return (a.x * b.x) + (a.y * b.y);
        }

        private bool CanFit(Vector3Int origin, Vector2Int size, int unitId, GameEntity map)
        {
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
                        return false;

                    if (reservedField.TryGetValue(checkPos, out var resId) && resId != unitId)
                        return false;
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

                if (!CanFit(corner1, size, unitId, map) || !CanFit(corner2, size, unitId, map))
                    return true;
            }

            return false;
        }

        private struct StepLimits
        {
            public Dictionary<Vector3Int, int> Integration;
            public bool CapCost;
            public int MaxCost;
            public bool MeasureDistance;
            public int MaxDistance;
            public Vector3Int Goal;
            public bool AlongCommit;
            public bool PreferIdeal;
            public Vector3Int IdealStep;
            public float DirectionBonus;
        }
    }
}
