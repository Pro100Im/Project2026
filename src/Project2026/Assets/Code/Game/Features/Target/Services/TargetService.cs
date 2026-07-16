using Code.Game.Features.Attack;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Target.Services
{
    public class TargetService
    {
        public const float CellSize = 0.4f;

        private const float MinEffectiveRange = 1.5f;

        private readonly List<Vector3Int> _neighborsBuffer = new(8);

        public static void GetFootprint(GameEntity target, out int minX, out int minY, out int maxX, out int maxY)
        {
            var origin = target.currentCell.Value;
            var footprintSize = target.hasUnitSize ? target.unitSize.Value : Vector2Int.one;

            minX = origin.x;
            minY = origin.y;
            maxX = origin.x + footprintSize.x - 1;
            maxY = origin.y + footprintSize.y - 1;
        }

        public static float GetEffectiveRange(float range)
        {
            return Mathf.Max(range, MinEffectiveRange);
        }

        public static float GetPhysicalRange(float range) => GetEffectiveRange(range) * CellSize;

        public static Vector2 GetClosestPoint(GameEntity target, Vector2 fromPoint) =>
            target.hasBounds
                ? (Vector2)target.bounds.Value.bounds.ClosestPoint(fromPoint)
                : (Vector2)target.woldPos.Value;

        public static int GetSurroundMaxRing(float range)
        {
            return Mathf.CeilToInt(GetEffectiveRange(range));
        }

        private readonly List<Vector3Int> _slotCandidatesBuffer = new(64);

        public bool TryPickSurroundSlot(
            Vector3Int unitCell,
            GameEntity target,
            float range,
            Vector2Int size,
            int unitId,
            GameEntity map,
            Dictionary<Vector3Int, Vector3> tilemap,
            Dictionary<Vector3Int, int> surroundField,
            out Vector3Int bestSlot)
        {
            GetFootprint(target, out var minX, out var minY, out var maxX, out var maxY);

            return TryPickSurroundSlot(
                unitCell,
                target,
                minX,
                minY,
                maxX,
                maxY,
                range,
                size,
                unitId,
                map,
                tilemap,
                surroundField,
                out bestSlot);
        }

        public bool TryPickSurroundSlot(
            Vector3Int unitCell,
            GameEntity target,
            int minX,
            int minY,
            int maxX,
            int maxY,
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
            var maxRing = GetSurroundMaxRing(range);
            var physicalRange = GetPhysicalRange(range);
            var sqrPhysicalRange = physicalRange * physicalRange;
            var found = false;

            _slotCandidatesBuffer.Clear();

            for (var ring = 1; ring <= maxRing; ring++)
            {
                var oMinX = minX - ring;
                var oMaxX = maxX + ring;
                var oMinY = minY - ring;
                var oMaxY = maxY + ring;

                _slotCandidatesBuffer.Clear();

                for (var x = oMinX; x <= oMaxX; x++)
                {
                    for (var y = oMinY; y <= oMaxY; y++)
                    {
                        var onBorder = x == oMinX || x == oMaxX || y == oMinY || y == oMaxY;

                        if (!onBorder)
                            continue;

                        var candidate = new Vector3Int(x, y, 0);

                        if (!tilemap.TryGetValue(candidate, out var candidateWorldPos))
                            continue;

                        var targetPoint = GetClosestPoint(target, candidateWorldPos);
                        var dx = candidateWorldPos.x - targetPoint.x;
                        var dy = candidateWorldPos.y - targetPoint.y;

                        if ((dx * dx) + (dy * dy) > sqrPhysicalRange)
                            continue;

                        if (surroundField.TryGetValue(candidate, out var ownerId) && ownerId != unitId)
                            continue;

                        if (!CanFitSlot(candidate, size, unitId, map))
                            continue;

                        _slotCandidatesBuffer.Add(candidate);
                    }
                }

                if (_slotCandidatesBuffer.Count > 0)
                    break;
            }

            for (var i = 0; i < _slotCandidatesBuffer.Count; i++)
            {
                var candidate = _slotCandidatesBuffer[i];
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

        public static bool CanFitSlot(Vector3Int origin, Vector2Int size, int unitId, GameEntity map)
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

        public List<Vector3Int> GetNeighbors(Vector3Int cell)
        {
            _neighborsBuffer.Clear();

            _neighborsBuffer.Add(new Vector3Int(cell.x + 1, cell.y, 0));
            _neighborsBuffer.Add(new Vector3Int(cell.x - 1, cell.y, 0));
            _neighborsBuffer.Add(new Vector3Int(cell.x, cell.y + 1, 0));
            _neighborsBuffer.Add(new Vector3Int(cell.x, cell.y - 1, 0));
            _neighborsBuffer.Add(new Vector3Int(cell.x + 1, cell.y + 1, 0));
            _neighborsBuffer.Add(new Vector3Int(cell.x - 1, cell.y + 1, 0));
            _neighborsBuffer.Add(new Vector3Int(cell.x + 1, cell.y - 1, 0));
            _neighborsBuffer.Add(new Vector3Int(cell.x - 1, cell.y - 1, 0));

            return _neighborsBuffer;
        }

        public AttackDirection GetAttackDirection(Vector3 closestA, Vector3 closestB)
        {
            var dir = closestB - closestA;

            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                return AttackDirection.Side;
            else
                return dir.y > 0 ? AttackDirection.Up : AttackDirection.Down;
        }

        public Vector3 GetInterceptPoint(Vector3 shooterPos, float projectileSpeed, Vector3 targetPos, Vector3 targetVelocity)
        {
            var relativePosition = targetPos - shooterPos;
            var relativeVelocity = targetVelocity;

            var a = Vector3.Dot(relativeVelocity, relativeVelocity) - (projectileSpeed * projectileSpeed);
            var b = 2f * Vector3.Dot(relativeVelocity, relativePosition);
            var c = Vector3.Dot(relativePosition, relativePosition);

            var determinant = b * b - 4f * a * c;

            if (determinant > 0)
            {
                var t1 = (-b + Mathf.Sqrt(determinant)) / (2f * a);
                var t2 = (-b - Mathf.Sqrt(determinant)) / (2f * a);
                var t = 0f;

                if (t1 > 0 && t2 > 0) 
                    t = Mathf.Min(t1, t2);
                else if (t1 > 0) 
                    t = t1;
                else if (t2 > 0) 
                    t = t2;

                if (t > 0)
                    return targetPos + targetVelocity * t;
            }

            return targetPos;
        }
    }
}