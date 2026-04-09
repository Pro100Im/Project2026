using UnityEngine;

namespace Code.Game.Features.Target.Services
{
    public class TargetService
    {
        public Vector2 ClosestPointAABB2D(Vector2 point, Vector2 center, Vector2 size)
        {
            var half = size * 0.5f;

            return new Vector2(
                Mathf.Clamp(point.x, center.x - half.x, center.x + half.x),
                Mathf.Clamp(point.y, center.y - half.y, center.y + half.y)
            );
        }

        public float GetDistanceBetweenEntities(Bounds ba, Bounds bb, Vector2 closestB, Vector2 closestA)
        {
            var dist1 = Vector2.Distance(ba.center, closestB);
            var dist2 = Vector2.Distance(bb.center, closestA);

            return Mathf.Min(dist1, dist2);
        }
    }
}