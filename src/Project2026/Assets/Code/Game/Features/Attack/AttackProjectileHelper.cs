using Code.Game.Common.Entity;
using Code.Game.Features.Target.Services;
using Code.Game.StaticData.Data;
using Code.Game.StaticData.Property;
using UnityEngine;

namespace Code.Game.Features.Attack
{
    public static class AttackProjectileHelper
    {
        public static float GetArcHeight(float baseArcHeight, float distance) =>
            baseArcHeight * Mathf.Clamp01(distance / 10f);

        public static bool CanFire(GameEntity owner) =>
            TryGetShot(owner, out _, out _, out _);

        public static bool TryFire(GameEntity owner)
        {
            if (!TryGetShot(owner, out var aimPoint, out var totalDistance, out var dynamicArcHeight))
                return false;

            var firePoint = GetFirePoint(owner);
            var projectile = CreateGameEntity.Empty();

            projectile.AddOwnerId(owner.id.Value);
            projectile.AddTargetId(owner.targetId.Value);
            projectile.AddSpawnPosition(firePoint);
            projectile.AddAttackerPoint(firePoint);
            projectile.AddTeam(owner.team.Value);
            projectile.isMovementAvailable = true;

            foreach (var property in owner.projectile.Value.Properties)
                property.Apply(projectile);

            projectile.AddTargetPoint(aimPoint);
            projectile.AddTotalDistance(totalDistance);
            projectile.ReplaceTrajectoryCurrentArcHeight(dynamicArcHeight);

            return true;
        }

        private static bool TryGetShot(
            GameEntity owner,
            out Vector3 aimPoint,
            out float totalDistance,
            out float dynamicArcHeight)
        {
            aimPoint = default;
            totalDistance = 0f;
            dynamicArcHeight = 0f;

            if (!owner.hasProjectile || !owner.hasFirePointOffset || !owner.hasWoldPos || !owner.hasTargetPoint || !owner.hasRange)
                return false;

            var projSpeed = 0f;
            var baseArcHeight = 0f;

            foreach (var property in owner.projectile.Value.Properties)
            {
                if (property is SpeedProperty speedProperty)
                    projSpeed = speedProperty.Speed;
                else if (property is TrajectoryProperty trajectoryProperty)
                    baseArcHeight = trajectoryProperty.ArcHeight;
            }

            if (projSpeed <= 0f)
                return false;

            var firePoint = GetFirePoint(owner);
            aimPoint = owner.targetPoint.Value;

            var dx = aimPoint.x - firePoint.x;
            var dy = aimPoint.y - firePoint.y;
            var sqrDistance = (dx * dx) + (dy * dy);
            var physicalRange = TargetService.GetPhysicalRange(owner.range.Value);

            if (sqrDistance > physicalRange * physicalRange)
                return false;

            totalDistance = Mathf.Sqrt(sqrDistance);
            dynamicArcHeight = GetArcHeight(baseArcHeight, totalDistance);

            return true;
        }

        private static Vector3 GetFirePoint(GameEntity owner) => owner.woldPos.Value + owner.firePointOffset.Value;
    }
}
