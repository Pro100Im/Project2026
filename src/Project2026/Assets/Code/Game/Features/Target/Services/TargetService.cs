using UnityEngine;

namespace Code.Game.Features.Target.Services
{
    public class TargetService
    {
        public float GetDistanceBetweenEntities(Vector2 closestB, Vector2 closestA)
        {
            return Vector2.Distance(closestA, closestB);
        }
    }
}