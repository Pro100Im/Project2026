using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Game.Features.Target.Services
{
    public class TargetService
    {
        public float GetDistanceBetweenEntities(Vector2 closestB, Vector2 closestA)
        {
            return Vector2.Distance(closestA, closestB);
        }

        public List<Vector3Int> FindPath(int unitId, Vector3Int start, Vector3Int targetCell,
         Dictionary<Vector3Int, Vector3> tilemapMovement,
         Dictionary<Vector3Int, int> occupancyMap,
         Vector2Int unitSize)
        {
            var goalCell = FindAccessibleGoal(unitId, targetCell, unitSize, tilemapMovement, occupancyMap);

            if (goalCell.x == int.MinValue) return new List<Vector3Int>();
            if (goalCell == start) return new List<Vector3Int>();

            var openSet = new List<Vector3Int> { start };
            var cameFrom = new Dictionary<Vector3Int, Vector3Int>();

            var gScore = new Dictionary<Vector3Int, float> { [start] = 0 };
            var fScore = new Dictionary<Vector3Int, float> { [start] = Heuristic(start, goalCell) };

            while (openSet.Count > 0)
            {
                var current = openSet.OrderBy(c => fScore.ContainsKey(c) ? fScore[c] : float.MaxValue).First();

                if (current == goalCell)
                    return ReconstructPath(cameFrom, current);

                openSet.Remove(current);

                foreach (var neighbor in GetNeighbors(current))
                {
                    if (!tilemapMovement.ContainsKey(neighbor)) continue;
                    if (!CanOccupy(unitId, neighbor, unitSize, tilemapMovement, occupancyMap)) continue;

                    float moveCost = (current.x != neighbor.x && current.y != neighbor.y) ? 1.41f : 1.0f;
                    var tentativeG = gScore[current] + moveCost;

                    if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        fScore[neighbor] = tentativeG + Heuristic(neighbor, goalCell);

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }

            return new List<Vector3Int>();
        }

        private Vector3Int FindAccessibleGoal(int unitId, Vector3Int targetCell, Vector2Int unitSize,
            Dictionary<Vector3Int, Vector3> tilemapMovement, Dictionary<Vector3Int, int> occupancyMap)
        {
            var candidates = new List<Vector3Int>();
            for (int x = -unitSize.x; x <= 1; x++)
            {
                for (int y = -unitSize.y; y <= 1; y++)
                {
                    candidates.Add(new Vector3Int(targetCell.x + x, targetCell.y + y, 0));
                }
            }

            return candidates
                .OrderBy(c => Vector3Int.Distance(c, targetCell))
                .FirstOrDefault(c => CanOccupy(unitId, c, unitSize, tilemapMovement, occupancyMap, ignoreTarget: true));
        }

        private bool CanOccupy(int unitId, Vector3Int baseCell, Vector2Int unitSize,
            Dictionary<Vector3Int, Vector3> tilemapMovement, Dictionary<Vector3Int, int> occupancyMap, bool ignoreTarget = false)
        {
            for (int x = 0; x < unitSize.x; x++)
            {
                for (int y = 0; y < unitSize.y; y++)
                {
                    var checkCell = new Vector3Int(baseCell.x + x, baseCell.y + y, 0);

                    if (!tilemapMovement.ContainsKey(checkCell)) return false;

                    if (occupancyMap.TryGetValue(checkCell, out var ownerId))
                    {
                        if (ownerId != unitId) return false;
                    }
                }
            }
            return true;
        }

        private float Heuristic(Vector3Int a, Vector3Int b)
        {
            return Math.Max(Math.Abs(a.x - b.x), Math.Abs(a.y - b.y));
        }

        private List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
        {
            var path = new List<Vector3Int> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }
            path.Reverse();
            return path;
        }

        private IEnumerable<Vector3Int> GetNeighbors(Vector3Int cell)
        {
            yield return new Vector3Int(cell.x + 1, cell.y, 0);
            yield return new Vector3Int(cell.x - 1, cell.y, 0);
            yield return new Vector3Int(cell.x, cell.y + 1, 0);
            yield return new Vector3Int(cell.x, cell.y - 1, 0);

            yield return new Vector3Int(cell.x + 1, cell.y + 1, 0);
            yield return new Vector3Int(cell.x - 1, cell.y - 1, 0);
            yield return new Vector3Int(cell.x + 1, cell.y - 1, 0);
            yield return new Vector3Int(cell.x - 1, cell.y + 1, 0);
        }
    }
}