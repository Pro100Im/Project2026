using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Target.Services
{
    public class TargetService
    {
        public float GetDistanceBetweenEntities(Vector2 closestB, Vector2 closestA)
        {
            return Vector2.Distance(closestA, closestB);
        }

        public IEnumerable<Vector3Int> GetNeighbors(Vector3Int cell)
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

        public IEnumerable<Vector3Int> GetCardinalNeighbors(Vector3Int cell)
        {
            yield return new Vector3Int(cell.x + 1, cell.y, 0);
            yield return new Vector3Int(cell.x - 1, cell.y, 0);
            yield return new Vector3Int(cell.x, cell.y + 1, 0);
            yield return new Vector3Int(cell.x, cell.y - 1, 0);
        }
    }
}