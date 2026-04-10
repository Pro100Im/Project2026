using UnityEngine;

namespace Code.Game.Features.Target.Services
{
    public class TargetService
    {
        public float GetDistanceBetweenEntities(Bounds ba, Bounds bb, Vector2 closestB, Vector2 closestA)
        {
            return Vector2.Distance(closestA, closestB);
        }
    }
}