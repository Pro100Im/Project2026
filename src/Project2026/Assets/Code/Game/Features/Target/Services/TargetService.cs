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

        public static int GetFootprintRing(
            Vector3Int origin,
            Vector2Int size,
            int tMinX,
            int tMinY,
            int tMaxX,
            int tMaxY)
        {
            var aMinX = origin.x;
            var aMinY = origin.y;
            var aMaxX = origin.x + size.x - 1;
            var aMaxY = origin.y + size.y - 1;

            var gapX = IntervalGap(aMinX, aMaxX, tMinX, tMaxX);
            var gapY = IntervalGap(aMinY, aMaxY, tMinY, tMaxY);

            return Mathf.Max(gapX, gapY);
        }

        private static int IntervalGap(int aMin, int aMax, int bMin, int bMax)
        {
            if (aMax < bMin)
                return bMin - aMax;

            if (bMax < aMin)
                return aMin - bMax;

            return 0;
        }

        public static void GetOriginBlockedAabb(
            int tMinX,
            int tMinY,
            int tMaxX,
            int tMaxY,
            Vector2Int size,
            out int bMinX,
            out int bMinY,
            out int bMaxX,
            out int bMaxY)
        {
            var padX = size.x - 1;
            var padY = size.y - 1;

            bMinX = tMinX - padX;
            bMinY = tMinY - padY;
            bMaxX = tMaxX;
            bMaxY = tMaxY;
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
            return Mathf.Max(1, Mathf.FloorToInt(GetEffectiveRange(range)));
        }

        public static int GetRangedMinRing(float range)
        {
            return Mathf.Max(2, Mathf.CeilToInt(GetEffectiveRange(range) * 0.5f));
        }

        public static float GetRangedMinSafePhysical(float range) => GetPhysicalRange(range) * 0.5f;

        public static bool IsTooCloseForRanged(GameEntity unit, GameEntity target)
        {
            var unitPos = unit.woldPos.Value;

            if (unit.hasUnitAnchorPoint)
                unitPos += unit.unitAnchorPoint.Value;

            var closest = GetClosestPoint(target, unitPos);
            var dx = unitPos.x - closest.x;
            var dy = unitPos.y - closest.y;
            var minSafe = GetRangedMinSafePhysical(unit.range.Value);

            return (dx * dx) + (dy * dy) < minSafe * minSafe;
        }

        public static float GetSqrDistanceToTarget(Vector3 worldPos, GameEntity target)
        {
            var closest = GetClosestPoint(target, worldPos);
            var dx = worldPos.x - closest.x;
            var dy = worldPos.y - closest.y;

            return (dx * dx) + (dy * dy);
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
            out Vector3Int bestSlot,
            bool preferMaxRange = false)
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
                out bestSlot,
                preferMaxRange);
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
            out Vector3Int bestSlot,
            bool preferMaxRange = false)
        {
            bestSlot = default;
            var maxRing = GetSurroundMaxRing(range);
            var physicalRange = GetPhysicalRange(range);
            var sqrPhysicalRange = physicalRange * physicalRange;

            GetOriginBlockedAabb(minX, minY, maxX, maxY, size, out var bMinX, out var bMinY, out var bMaxX, out var bMaxY);

            if (TryClaimCurrentCellAsSlot(
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
                    preferMaxRange,
                    out bestSlot))
                return true;

            if (preferMaxRange)
            {
                var minPreferredRing = Mathf.Min(GetRangedMinRing(range), maxRing);

                if (TryCollectBestSlotOnRings(
                        unitCell,
                        target,
                        bMinX,
                        bMinY,
                        bMaxX,
                        bMaxY,
                        maxRing,
                        minPreferredRing,
                        -1,
                        sqrPhysicalRange,
                        size,
                        unitId,
                        map,
                        tilemap,
                        surroundField,
                        collectAllRings: false,
                        out bestSlot))
                    return true;

                if (minPreferredRing > 1
                    && TryCollectBestSlotOnRings(
                        unitCell,
                        target,
                        bMinX,
                        bMinY,
                        bMaxX,
                        bMaxY,
                        minPreferredRing - 1,
                        1,
                        -1,
                        sqrPhysicalRange,
                        size,
                        unitId,
                        map,
                        tilemap,
                        surroundField,
                        collectAllRings: false,
                        out bestSlot))
                    return true;

                return false;
            }

            return TryCollectBestSlotOnRings(
                unitCell,
                target,
                bMinX,
                bMinY,
                bMaxX,
                bMaxY,
                1,
                maxRing,
                1,
                sqrPhysicalRange,
                size,
                unitId,
                map,
                tilemap,
                surroundField,
                collectAllRings: true,
                out bestSlot);
        }

        private static bool TryClaimCurrentCellAsSlot(
            Vector3Int unitCell,
            GameEntity target,
            int tMinX,
            int tMinY,
            int tMaxX,
            int tMaxY,
            float range,
            Vector2Int size,
            int unitId,
            GameEntity map,
            Dictionary<Vector3Int, Vector3> tilemap,
            Dictionary<Vector3Int, int> surroundField,
            bool preferMaxRange,
            out Vector3Int bestSlot)
        {
            bestSlot = default;

            var ring = GetFootprintRing(unitCell, size, tMinX, tMinY, tMaxX, tMaxY);

            if (ring < 1 || ring > GetSurroundMaxRing(range))
                return false;

            if (surroundField.TryGetValue(unitCell, out var ownerId) && ownerId != unitId)
                return false;

            if (!CanFitSlot(unitCell, size, unitId, map))
                return false;

            if (!TryGetClosestFootprintSqrDistance(unitCell, size, target, tilemap, out var sqrDist))
                return false;

            var physicalRange = GetPhysicalRange(range);

            if (sqrDist > physicalRange * physicalRange)
                return false;

            if (preferMaxRange)
            {
                var minSafe = GetRangedMinSafePhysical(range);

                if (sqrDist < minSafe * minSafe)
                    return false;
            }

            bestSlot = unitCell;
            return true;
        }

        private bool TryCollectBestSlotOnRings(
            Vector3Int unitCell,
            GameEntity target,
            int bMinX,
            int bMinY,
            int bMaxX,
            int bMaxY,
            int startRing,
            int endRing,
            int step,
            float sqrPhysicalRange,
            Vector2Int size,
            int unitId,
            GameEntity map,
            Dictionary<Vector3Int, Vector3> tilemap,
            Dictionary<Vector3Int, int> surroundField,
            bool collectAllRings,
            out Vector3Int bestSlot)
        {
            bestSlot = default;
            var bestDist = int.MaxValue;
            var found = false;

            _slotCandidatesBuffer.Clear();

            for (var ring = startRing; step > 0 ? ring <= endRing : ring >= endRing; ring += step)
            {
                var oMinX = bMinX - ring;
                var oMaxX = bMaxX + ring;
                var oMinY = bMinY - ring;
                var oMaxY = bMaxY + ring;

                if (!collectAllRings)
                    _slotCandidatesBuffer.Clear();

                var ringCandidateCount = 0;

                for (var x = oMinX; x <= oMaxX; x++)
                {
                    for (var y = oMinY; y <= oMaxY; y++)
                    {
                        var onBorder = x == oMinX || x == oMaxX || y == oMinY || y == oMaxY;

                        if (!onBorder)
                            continue;

                        var candidate = new Vector3Int(x, y, 0);

                        if (!tilemap.ContainsKey(candidate))
                            continue;

                        if (!TryGetClosestFootprintSqrDistance(candidate, size, target, tilemap, out var sqrDist))
                            continue;

                        if (sqrDist > sqrPhysicalRange)
                            continue;

                        if (surroundField.TryGetValue(candidate, out var ownerId) && ownerId != unitId)
                            continue;

                        if (!CanFitSlot(candidate, size, unitId, map))
                            continue;

                        _slotCandidatesBuffer.Add(candidate);
                        ringCandidateCount++;
                    }
                }

                if (!collectAllRings && ringCandidateCount > 0)
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

        private static bool TryGetClosestFootprintSqrDistance(
            Vector3Int origin,
            Vector2Int size,
            GameEntity target,
            Dictionary<Vector3Int, Vector3> tilemap,
            out float sqrDist)
        {
            sqrDist = float.MaxValue;
            var found = false;

            for (var x = 0; x < size.x; x++)
            {
                for (var y = 0; y < size.y; y++)
                {
                    var cell = new Vector3Int(origin.x + x, origin.y + y, 0);

                    if (!tilemap.TryGetValue(cell, out var cellWorldPos))
                        continue;

                    var targetPoint = GetClosestPoint(target, cellWorldPos);
                    var dx = cellWorldPos.x - targetPoint.x;
                    var dy = cellWorldPos.y - targetPoint.y;
                    var cellSqr = (dx * dx) + (dy * dy);

                    if (cellSqr < sqrDist)
                        sqrDist = cellSqr;

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