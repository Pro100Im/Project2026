using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Target.Systems
{
    public class AssignSurroundSlotSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _units;
        private readonly IGroup<GameEntity> _maps;

        private readonly List<GameEntity> _buffer = new(256);
        private readonly HashSet<int> _processedTargets = new(256);
        private readonly List<Vector3Int> _candidates = new(64);

        public AssignSurroundSlotSystem(GameContext context)
        {
            _units = context.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.GridMovement,
                    GameMatcher.CurrentCell,
                    GameMatcher.Id,
                    GameMatcher.Team,
                    GameMatcher.Range,
                    GameMatcher.UnitSize,
                    GameMatcher.MovementAvailable)
                .NoneOf(
                    GameMatcher.SurroundSlot,
                    GameMatcher.Dead));

            _maps = context.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.SpatialHash,
                    GameMatcher.TilemapMovement,
                    GameMatcher.SurroundField,
                    GameMatcher.OccupField,
                    GameMatcher.ReservedField));
        }

        public void Execute()
        {
            var mapEntity = _maps.GetSingleEntity();

            if (mapEntity == null)
                return;

            var spatialHash = mapEntity.spatialHash.Value;
            var tilemap = mapEntity.tilemapMovement.Value;
            var surroundField = mapEntity.surroundField.Value;
            var units = _units.GetEntities(_buffer);

            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                var unitCell = unit.currentCell.Value;
                var unitId = unit.id.Value;
                var myTeam = unit.team.Value;
                var size = unit.unitSize.Value;
                var range = unit.range.Value;
                var iRange = Mathf.CeilToInt(range);

                var bestTargetId = -1;
                var bestTargetCell = Vector3Int.zero;
                var closestSqrDist = float.MaxValue;

                _processedTargets.Clear();

                for (var x = -iRange; x <= iRange; x++)
                {
                    for (var y = -iRange; y <= iRange; y++)
                    {
                        var checkPos = new Vector2Int(unitCell.x + x, unitCell.y + y);

                        if (!spatialHash.TryGetValue(checkPos, out var potentialTargets))
                            continue;

                        for (var j = 0; j < potentialTargets.Count; j++)
                        {
                            var targetId = potentialTargets[j];

                            if (targetId == unitId || !_processedTargets.Add(targetId))
                                continue;

                            var target = GetGameEntityById.Get(targetId);

                            if (target == null || target.team.Value == myTeam || !target.isTargetable || target.isDead)
                                continue;

                            if (!target.hasCurrentCell)
                                continue;

                            var targetCell = target.currentCell.Value;
                            var dx = targetCell.x - unitCell.x;
                            var dy = targetCell.y - unitCell.y;
                            var chebyshev = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));

                            if (chebyshev > range)
                                continue;

                            var sDist = (dx * dx) + (dy * dy);

                            if (sDist < closestSqrDist)
                            {
                                closestSqrDist = sDist;
                                bestTargetId = targetId;
                                bestTargetCell = targetCell;
                            }
                        }
                    }
                }

                if (bestTargetId == -1)
                    continue;

                if (!TryPickSlot(unitCell, bestTargetCell, range, size, unitId, mapEntity, tilemap, surroundField, out var slot))
                    continue;

                surroundField[slot] = unitId;
                unit.AddSurroundSlot(slot);
                unit.AddSurroundTargetId(bestTargetId);
            }
        }

        private bool TryPickSlot(
            Vector3Int unitCell,
            Vector3Int targetCell,
            float range,
            Vector2Int size,
            int unitId,
            GameEntity map,
            Dictionary<Vector3Int, Vector3> tilemap,
            Dictionary<Vector3Int, int> surroundField,
            out Vector3Int bestSlot)
        {
            bestSlot = default;
            var bestDist = int.MaxValue;
            var maxRing = Mathf.CeilToInt(range);
            var found = false;

            _candidates.Clear();

            for (var ring = 1; ring <= maxRing; ring++)
            {
                for (var dx = -ring; dx <= ring; dx++)
                {
                    for (var dy = -ring; dy <= ring; dy++)
                    {
                        if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) != ring)
                            continue;

                        var candidate = new Vector3Int(targetCell.x + dx, targetCell.y + dy, 0);

                        if (!tilemap.ContainsKey(candidate))
                            continue;

                        if (surroundField.TryGetValue(candidate, out var ownerId) && ownerId != unitId)
                            continue;

                        if (!CanFit(candidate, size, unitId, map))
                            continue;

                        _candidates.Add(candidate);
                    }
                }
            }

            for (var i = 0; i < _candidates.Count; i++)
            {
                var candidate = _candidates[i];
                var dx = candidate.x - unitCell.x;
                var dy = candidate.y - unitCell.y;
                var dist = (dx * dx) + (dy * dy);

                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestSlot = candidate;
                    found = true;
                }
            }

            return found;
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
    }
}
