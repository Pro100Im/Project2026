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

        public List<Vector3Int> AStar(Vector3Int start, Vector3Int targetCell, Dictionary<Vector3Int, Vector3> tilemapMovement, 
            Dictionary<Vector3Int, int> occupancyMap, Vector2Int unitSize)
        {
            foreach (var neighbor in GetNeighbors(targetCell))
            {
                if (neighbor == start)
                    return new();
            }

            var goalCell = FindAccessibleGoal(targetCell, unitSize, tilemapMovement, occupancyMap);

            if (goalCell.x == int.MinValue)
                return new();

            var openSet = new HashSet<Vector3Int> { start };
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
                    if (!tilemapMovement.ContainsKey(neighbor))
                        continue;

                    if (!CanOccupy(neighbor, unitSize, tilemapMovement, occupancyMap))
                        continue;

                    var tentativeG = gScore[current] + Vector3.Distance(tilemapMovement[current], tilemapMovement[neighbor]);

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

            return new();
        }


        private Vector3Int FindAccessibleGoal(Vector3Int targetCell, Vector2Int unitSize, Dictionary<Vector3Int, Vector3> tilemapMovement, Dictionary<Vector3Int, int> occupancyMap)
        {
            var neighbors = GetNeighbors(targetCell);

            foreach (var neighbor in neighbors.OrderBy(n => Vector3Int.Distance(n, targetCell)))
            {
                if (!tilemapMovement.ContainsKey(neighbor))
                    continue;

                if (CanOccupy(neighbor, unitSize, tilemapMovement, occupancyMap))
                    return neighbor;
            }

            return new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
        }



        private float Heuristic(Vector3Int a, Vector3Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
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
            yield return new Vector3Int(cell.x - 1, cell.y + 1, 0);
            yield return new Vector3Int(cell.x + 1, cell.y - 1, 0);
            yield return new Vector3Int(cell.x - 1, cell.y - 1, 0);
        }

        private bool CanOccupy(Vector3Int baseCell, Vector2Int unitSize, Dictionary<Vector3Int, Vector3> tilemapMovement, Dictionary<Vector3Int, int> occupancyMap)
        {
            for (int x = 0; x < unitSize.x; x++)
            {
                for (int y = 0; y < unitSize.y; y++)
                {
                    var checkCell = new Vector3Int(baseCell.x + x, baseCell.y + y, 0);

                    if (!tilemapMovement.ContainsKey(checkCell))
                        return false;

                    if (occupancyMap.ContainsKey(checkCell))
                        return false;
                }
            }

            return true;
        }
    }
}