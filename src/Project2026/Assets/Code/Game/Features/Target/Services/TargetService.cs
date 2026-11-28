using Code.Game.Features.Attack;
using Code.Game.Features.Level;
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
                ? ClosestPointOnSprite(target.bounds.Value, fromPoint)
                : (Vector2)target.woldPos.Value;

        /// <summary>
        /// Closest point on the sprite's world rect, computed from sprite.bounds and the transform. The bounds
        /// renderer is kept disabled (it is never drawn), and Renderer.bounds is not maintained for disabled
        /// renderers, so it must not be used here. Assumes no rotation, which holds for all bounds sprites.
        /// </summary>
        private static Vector2 ClosestPointOnSprite(SpriteRenderer renderer, Vector2 fromPoint)
        {
            var transform = renderer.transform;
            var sprite = renderer.sprite;

            if (sprite == null)
                return transform.position;

            var local = sprite.bounds;
            var scale = transform.lossyScale;
            var centerX = renderer.flipX ? -local.center.x : local.center.x;
            var centerY = renderer.flipY ? -local.center.y : local.center.y;
            var position = transform.position;

            var cx = position.x + (centerX * scale.x);
            var cy = position.y + (centerY * scale.y);
            var ex = Mathf.Abs(local.extents.x * scale.x);
            var ey = Mathf.Abs(local.extents.y * scale.y);

            return new Vector2(
                Mathf.Clamp(fromPoint.x, cx - ex, cx + ex),
                Mathf.Clamp(fromPoint.y, cy - ey, cy + ey));
        }

        public static Vector2 GetClosestPoint(
            GameEntity target,
            LevelArena arena,
            Vector2 fromPoint)
        {
            var boundsPoint = GetClosestPoint(target, fromPoint);

            if (!TryGetFootprintClosestPoint(target, arena, fromPoint, out var footprintPoint))
                return boundsPoint;

            var boundsDx = fromPoint.x - boundsPoint.x;
            var boundsDy = fromPoint.y - boundsPoint.y;
            var boundsSqr = (boundsDx * boundsDx) + (boundsDy * boundsDy);

            var footprintDx = fromPoint.x - footprintPoint.x;
            var footprintDy = fromPoint.y - footprintPoint.y;
            var footprintSqr = (footprintDx * footprintDx) + (footprintDy * footprintDy);

            return footprintSqr <= boundsSqr ? footprintPoint : boundsPoint;
        }

        private static bool TryGetFootprintClosestPoint(
            GameEntity target,
            LevelArena arena,
            Vector2 fromPoint,
            out Vector2 closest)
        {
            closest = default;

            if (!target.hasCurrentCell || !target.hasUnitSize)
                return false;

            var size = target.unitSize.Value;

            if (size.x <= 1 && size.y <= 1)
                return false;

            GetFootprint(target, out var minX, out var minY, out var maxX, out var maxY);

            var minCell = new Vector3Int(minX, minY, 0);
            var maxCell = new Vector3Int(maxX, maxY, 0);

            if (!arena.TryGetCellWorld(minCell, out var minWorld)
                || !arena.TryGetCellWorld(maxCell, out var maxWorld))
                return false;

            var half = CellSize * 0.5f;
            var rectMinX = Mathf.Min(minWorld.x, maxWorld.x) - half;
            var rectMinY = Mathf.Min(minWorld.y, maxWorld.y) - half;
            var rectMaxX = Mathf.Max(minWorld.x, maxWorld.x) + half;
            var rectMaxY = Mathf.Max(minWorld.y, maxWorld.y) + half;

            closest = new Vector2(
                Mathf.Clamp(fromPoint.x, rectMinX, rectMaxX),
                Mathf.Clamp(fromPoint.y, rectMinY, rectMaxY));

            return true;
        }

        public static int GetSurroundMaxRing(float range)
        {
            return Mathf.Max(1, Mathf.FloorToInt(GetEffectiveRange(range)));
        }

        public static bool IsOnAttackRing(GameEntity attacker, GameEntity target)
        {
            if (!attacker.hasCurrentCell || !attacker.hasRange || !target.hasCurrentCell)
                return false;

            var size = attacker.hasUnitSize ? attacker.unitSize.Value : Vector2Int.one;

            GetFootprint(target, out var minX, out var minY, out var maxX, out var maxY);

            var ring = GetFootprintRing(
                attacker.currentCell.Value,
                size,
                minX,
                minY,
                maxX,
                maxY);

            return ring >= 1 && ring <= GetSurroundMaxRing(attacker.range.Value);
        }

        public static int GetRangedMinRing(float range)
        {
            return Mathf.Max(2, Mathf.CeilToInt(GetEffectiveRange(range) * 0.5f));
        }

        public static float GetRangedMinSafePhysical(float range) => GetPhysicalRange(range) * 0.5f;

        public static bool IsTooCloseForRanged(
            GameEntity unit,
            GameEntity target,
            LevelArena arena)
        {
            var unitPos = unit.woldPos.Value;

            if (unit.hasUnitAnchorPoint)
                unitPos += unit.unitAnchorPoint.Value;

            var closest = GetClosestPoint(target, arena, unitPos);
            var dx = unitPos.x - closest.x;
            var dy = unitPos.y - closest.y;
            var minSafe = GetRangedMinSafePhysical(unit.range.Value);

            return (dx * dx) + (dy * dy) < minSafe * minSafe;
        }

        public static float GetSqrDistanceToTarget(
            Vector3 worldPos,
            GameEntity target,
            LevelArena arena)
        {
            var closest = GetClosestPoint(target, arena, worldPos);
            var dx = worldPos.x - closest.x;
            var dy = worldPos.y - closest.y;

            return (dx * dx) + (dy * dy);
        }

        public static bool CanFireFromCell(
            Vector3Int unitCell,
            GameEntity target,
            float range,
            Vector2Int size,
            LevelArena arena)
        {
            GetFootprint(target, out var minX, out var minY, out var maxX, out var maxY);

            var ring = GetFootprintRing(unitCell, size, minX, minY, maxX, maxY);

            if (ring < 1)
                return false;

            if (!TryGetClosestFootprintSqrDistance(unitCell, size, target, arena, out var sqrDist))
                return false;

            var physicalRange = GetPhysicalRange(range);

            return sqrDist <= physicalRange * physicalRange;
        }

        private readonly List<Vector3Int> _slotCandidatesBuffer = new(1024);

        public bool TryPickSurroundSlot(
            Vector3Int unitCell,
            GameEntity target,
            float range,
            Vector2Int size,
            int unitId,
            LevelArena arena,
            out Vector3Int bestSlot,
            bool preferMaxRange = false,
            bool holdFirePosition = false)
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
                arena,
                out bestSlot,
                preferMaxRange,
                holdFirePosition);
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
            LevelArena arena,
            out Vector3Int bestSlot,
            bool preferMaxRange = false,
            bool holdFirePosition = false)
        {
            bestSlot = default;
            var maxRing = GetSurroundMaxRing(range);
            var physicalRange = GetPhysicalRange(range);
            var sqrPhysicalRange = physicalRange * physicalRange;
            var sideCenterX = (minX + maxX) * 0.5f;
            var sideCenterY = (minY + maxY) * 0.5f;

            GetOriginBlockedAabb(minX, minY, maxX, maxY, size, out var bMinX, out var bMinY, out var bMaxX, out var bMaxY);

            if (holdFirePosition
                && CanFireFromCell(unitCell, target, range, size, arena)
                && CanFitSlot(unitCell, size, unitId, arena))
            {
                bestSlot = unitCell;
                return true;
            }

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
                    arena,
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
                        arena,
                        collectAllRings: false,
                        sameSideOnly: true,
                        sideCenterX,
                        sideCenterY,
                        out bestSlot))
                    return true;

                if (holdFirePosition)
                    return false;

                var currentRing = GetFootprintRing(unitCell, size, minX, minY, maxX, maxY);
                var nearestFartherRing = currentRing + 1;

                if (nearestFartherRing > minPreferredRing - 1)
                    return false;

                return TryCollectBestSlotOnRings(
                    unitCell,
                    target,
                    bMinX,
                    bMinY,
                    bMaxX,
                    bMaxY,
                    minPreferredRing - 1,
                    nearestFartherRing,
                    -1,
                    sqrPhysicalRange,
                    size,
                    unitId,
                    arena,
                    collectAllRings: false,
                    sameSideOnly: true,
                    sideCenterX,
                    sideCenterY,
                    out bestSlot);
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
                arena,
                collectAllRings: true,
                sameSideOnly: false,
                sideCenterX: 0f,
                sideCenterY: 0f,
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
            LevelArena arena,
            bool preferMaxRange,
            out Vector3Int bestSlot)
        {
            bestSlot = default;

            var ring = GetFootprintRing(unitCell, size, tMinX, tMinY, tMaxX, tMaxY);

            if (ring < 1 || ring > GetSurroundMaxRing(range))
                return false;

            // Occupying the cell has priority: steal a foreign surround claim on our own cell.
            if (!CanFitSlot(unitCell, size, unitId, arena))
                return false;

            if (!TryGetClosestFootprintSqrDistance(unitCell, size, target, arena, out var sqrDist))
                return false;

            var physicalRange = GetPhysicalRange(range);

            if (sqrDist > physicalRange * physicalRange)
                return false;

            if (preferMaxRange)
            {
                if (ring < GetRangedMinRing(range))
                    return false;

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
            LevelArena arena,
            bool collectAllRings,
            bool sameSideOnly,
            float sideCenterX,
            float sideCenterY,
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

                        if (!arena.IsTile(candidate))
                            continue;

                        if (!TryGetClosestFootprintSqrDistance(candidate, size, target, arena, out var sqrDist))
                            continue;

                        if (sqrDist > sqrPhysicalRange)
                            continue;

                        var ownerId = arena.SurroundOwnerAt(candidate);

                        if (ownerId != 0 && ownerId != unitId)
                            continue;

                        if (!CanFitSlot(candidate, size, unitId, arena))
                            continue;

                        if (sameSideOnly && !IsSameSide(unitCell, candidate, sideCenterX, sideCenterY))
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

        private static bool IsSameSide(Vector3Int unitCell, Vector3Int candidate, float centerX, float centerY)
        {
            var unitDx = unitCell.x - centerX;
            var unitDy = unitCell.y - centerY;
            var slotDx = candidate.x - centerX;
            var slotDy = candidate.y - centerY;

            return (unitDx * slotDx) + (unitDy * slotDy) >= 0f;
        }

        private static bool TryGetClosestFootprintSqrDistance(
            Vector3Int origin,
            Vector2Int size,
            GameEntity target,
            LevelArena arena,
            out float sqrDist)
        {
            sqrDist = float.MaxValue;
            var found = false;

            for (var x = 0; x < size.x; x++)
            {
                for (var y = 0; y < size.y; y++)
                {
                    var cell = new Vector3Int(origin.x + x, origin.y + y, 0);

                    if (!arena.TryGetCellWorld(cell, out var cellWorldPos))
                        continue;

                    var targetPoint = GetClosestPoint(target, arena, cellWorldPos);
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

        public static bool CanFitSlot(Vector3Int origin, Vector2Int size, int unitId, LevelArena arena)
        {
            for (var x = 0; x < size.x; x++)
            {
                for (var y = 0; y < size.y; y++)
                {
                    var checkPos = new Vector3Int(origin.x + x, origin.y + y, 0);

                    if (!arena.IsTile(checkPos))
                        return false;

                    var occupant = arena.OccupantAt(checkPos);

                    if (occupant != 0 && occupant != unitId)
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
    }
}