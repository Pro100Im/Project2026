using Code.Game.Features.Level;
using UnityEngine;

namespace Code.Game.Features.Movement
{
    /// <summary>
    /// Unit against wall collision on the navigation grid. A wall is everything that is not a walkable cell:
    /// outside the tilemap, outside the grid, inside a building.
    /// Every change of a unit position goes through <see cref="Move"/>, which never returns a position that
    /// overlaps a wall, so a unit that starts in a valid place can not be pushed out of the map or into a building.
    /// </summary>
    public static class CrowdCollision
    {
        private const float Skin = 0.002f;
        private const int SettleSearchCells = 12;

        /// <summary>True when a circle does not touch any wall.</summary>
        public static bool IsFree(LevelArena arena, Vector2 center, float radius)
        {
            var cellSize = arena.CellSize;
            var minX = Mathf.FloorToInt((center.x - radius - arena.WorldMin.x) / cellSize);
            var maxX = Mathf.FloorToInt((center.x + radius - arena.WorldMin.x) / cellSize);
            var minY = Mathf.FloorToInt((center.y - radius - arena.WorldMin.y) / cellSize);
            var maxY = Mathf.FloorToInt((center.y + radius - arena.WorldMin.y) / cellSize);
            var limit = radius - Skin;
            var limitSqr = limit * limit;

            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    if (!IsWall(arena, x, y))
                        continue;

                    if (SqrDistanceToCell(arena, center, x, y) < limitSqr)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Moves a circle by a delta. Walls win: the circle slides along them, and if it can not move without
        /// touching a wall it stays where it is. The step is capped to half a cell so thin walls are never skipped.
        /// </summary>
        public static Vector2 Move(LevelArena arena, Vector2 position, Vector2 delta, float radius)
        {
            var maxStep = arena.CellSize * 0.5f;
            var sqrLength = delta.sqrMagnitude;

            if (sqrLength < 1e-10f)
                return position;

            if (sqrLength > maxStep * maxStep)
                delta *= maxStep / Mathf.Sqrt(sqrLength);

            var target = position + delta;

            if (TryResolve(arena, target, radius, out var resolved))
                return resolved;

            if (TryResolve(arena, new Vector2(target.x, position.y), radius, out resolved))
                return resolved;

            if (TryResolve(arena, new Vector2(position.x, target.y), radius, out resolved))
                return resolved;

            return position;
        }

        /// <summary>
        /// Finds a valid place for a circle at or near a position: used when a unit enters the simulation and
        /// when a building appears on top of it. Returns false only when there is no walkable cell nearby.
        /// </summary>
        public static bool TrySettle(LevelArena arena, Vector2 position, float radius, out Vector2 settled)
        {
            if (IsFree(arena, position, radius))
            {
                settled = position;
                return true;
            }

            if (TryResolve(arena, position, radius, out settled))
                return true;

            var cellSize = arena.CellSize;
            var originX = Mathf.FloorToInt((position.x - arena.WorldMin.x) / cellSize);
            var originY = Mathf.FloorToInt((position.y - arena.WorldMin.y) / cellSize);
            var bestSqr = float.MaxValue;
            var found = false;

            for (var ring = 0; ring <= SettleSearchCells && !found; ring++)
            {
                for (var y = originY - ring; y <= originY + ring; y++)
                {
                    for (var x = originX - ring; x <= originX + ring; x++)
                    {
                        if (Mathf.Max(Mathf.Abs(x - originX), Mathf.Abs(y - originY)) != ring)
                            continue;

                        if (IsWall(arena, x, y))
                            continue;

                        var center = new Vector2(
                            arena.WorldMin.x + ((x + 0.5f) * cellSize),
                            arena.WorldMin.y + ((y + 0.5f) * cellSize));

                        if (!IsFree(arena, center, radius))
                            continue;

                        var sqr = (center - position).sqrMagnitude;

                        if (sqr >= bestSqr)
                            continue;

                        bestSqr = sqr;
                        settled = center;
                        found = true;
                    }
                }
            }

            return found;
        }

        private static bool TryResolve(LevelArena arena, Vector2 center, float radius, out Vector2 resolved)
        {
            resolved = center;

            for (var pass = 0; pass < 3; pass++)
            {
                if (!PushOut(arena, ref resolved, radius))
                    return false;
            }

            return IsFree(arena, resolved, radius);
        }

        /// <summary>Pushes the circle out of every wall cell it overlaps. False when its centre is inside a wall.</summary>
        private static bool PushOut(LevelArena arena, ref Vector2 center, float radius)
        {
            var cellSize = arena.CellSize;
            var minX = Mathf.FloorToInt((center.x - radius - arena.WorldMin.x) / cellSize);
            var maxX = Mathf.FloorToInt((center.x + radius - arena.WorldMin.x) / cellSize);
            var minY = Mathf.FloorToInt((center.y - radius - arena.WorldMin.y) / cellSize);
            var maxY = Mathf.FloorToInt((center.y + radius - arena.WorldMin.y) / cellSize);

            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    if (!IsWall(arena, x, y))
                        continue;

                    var cellMinX = arena.WorldMin.x + (x * cellSize);
                    var cellMinY = arena.WorldMin.y + (y * cellSize);
                    var closestX = Mathf.Clamp(center.x, cellMinX, cellMinX + cellSize);
                    var closestY = Mathf.Clamp(center.y, cellMinY, cellMinY + cellSize);
                    var dx = center.x - closestX;
                    var dy = center.y - closestY;
                    var sqr = (dx * dx) + (dy * dy);

                    if (sqr >= radius * radius)
                        continue;

                    if (sqr < 1e-10f)
                        return false;

                    var distance = Mathf.Sqrt(sqr);
                    var push = (radius - distance) / distance;

                    center.x += dx * push;
                    center.y += dy * push;
                }
            }

            return true;
        }

        private static bool IsWall(LevelArena arena, int x, int y)
        {
            if (x < 0 || y < 0 || x >= arena.Width || y >= arena.Height)
                return true;

            return arena.Clearance[x + (y * arena.Width)] == 0;
        }

        private static float SqrDistanceToCell(LevelArena arena, Vector2 point, int x, int y)
        {
            var cellSize = arena.CellSize;
            var cellMinX = arena.WorldMin.x + (x * cellSize);
            var cellMinY = arena.WorldMin.y + (y * cellSize);
            var dx = point.x - Mathf.Clamp(point.x, cellMinX, cellMinX + cellSize);
            var dy = point.y - Mathf.Clamp(point.y, cellMinY, cellMinY + cellSize);

            return (dx * dx) + (dy * dy);
        }
    }
}
