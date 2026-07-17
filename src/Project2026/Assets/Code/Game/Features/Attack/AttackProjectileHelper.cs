using Code.Game.Common.Entity;
using Code.Game.Features.Target.Services;
using Code.Game.StaticData.Data;
using Code.Game.StaticData.Property;
using UnityEngine;

namespace Code.Game.Features.Attack
{
    public static class AttackProjectileHelper
    {
        public static bool CanFire(TargetService targetService, GameEntity owner, GameEntity target)
        {
            return TryGetIntercept(targetService, owner, target, out _, out _, out _);
        }

        public static bool TryFire(TargetService targetService, GameEntity owner, GameEntity target)
        {
            if (!TryGetIntercept(targetService, owner, target, out var interceptPoint, out var totalDistance, out var dynamicArcHeight))
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

            projectile.AddTargetPoint(interceptPoint);
            projectile.AddTotalDistance(totalDistance);
            projectile.ReplaceTrajectoryCurrentArcHeight(dynamicArcHeight);

            return true;
        }

        private static bool TryGetIntercept(
            TargetService targetService,
            GameEntity owner,
            GameEntity target,
            out Vector3 interceptPoint,
            out float totalDistance,
            out float dynamicArcHeight)
        {
            interceptPoint = default;
            totalDistance = 0f;
            dynamicArcHeight = 0f;

            if (!owner.hasProjectile || !owner.hasFirePointOffset || !owner.hasWoldPos || !owner.hasAttackerPoint || !owner.hasRange)
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

            var targetVel = target.hasVelocity ? target.velocity.Value : Vector3.zero;
            interceptPoint = targetService.GetInterceptPoint(
                owner.attackerPoint.Value,
                projSpeed,
                owner.targetPoint.Value,
                targetVel);

            var physicalRange = TargetService.GetPhysicalRange(owner.range.Value);
            var dx = interceptPoint.x - owner.attackerPoint.Value.x;
            var dy = interceptPoint.y - owner.attackerPoint.Value.y;

            if ((dx * dx) + (dy * dy) > physicalRange * physicalRange)
                return false;

            totalDistance = Vector3.Distance(GetFirePoint(owner), interceptPoint);
            var distanceFactor = Mathf.Clamp01(totalDistance / 10f);
            dynamicArcHeight = baseArcHeight * distanceFactor;

            return true;
        }

        private static Vector3 GetFirePoint(GameEntity owner) => owner.woldPos.Value + owner.firePointOffset.Value;
    }
}