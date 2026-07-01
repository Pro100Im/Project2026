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