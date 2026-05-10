using Code.Game.Common.Entity;
using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

// To do remove magic numbs
namespace Code.Game.Features.Attack.Systems
{
    public class RangeAttackEndSystem : IExecuteSystem
    {
        private readonly TargetService _targetService;

        private readonly IGroup<GameEntity> _rangeAttacks;
        private readonly List<GameEntity> _buffer = new(64);

        public RangeAttackEndSystem(GameContext gameContext, TargetService targetService)
        {
            _targetService = targetService;

            _rangeAttacks = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.OwnerId,
                    GameMatcher.Cooldown,
                    GameMatcher.Duration,
                    GameMatcher.RangeAttack));
        }

        public void Execute()
        {
            foreach (var attack in _rangeAttacks.GetEntities(_buffer))
            {
                if (attack.duration.Value > 0)
                    continue;

                var entity = GetGameEntityById.Get(attack.ownerId.Value);

                if (!entity.hasTargetId)
                {
                    entity.isAttacking = false;
                    entity.isAttackAvailable = true;

                    attack.isDestructed = true;

                    continue;
                }

                if (entity.isAttacking)
                    CreateProjectile(entity);

                entity.isAttacking = false;

                if (attack.cooldown.Value > 0)
                    continue;

                entity.isAttackAvailable = true;
                attack.isDestructed = true;
            }
        }

        private void CreateProjectile(GameEntity owner)
        {
            var projectile = CreateGameEntity.Empty();

            projectile.AddOwnerId(owner.id.Value);
            projectile.AddTargetId(owner.targetId.Value);
            projectile.AddSpawnPosition(owner.firePoint.Value);
            projectile.AddAttackerPoint(owner.firePoint.Value);
            projectile.isMovementAvailable = true;

            foreach (var property in owner.projectile.Value.Properties)
                property.Apply(projectile);

            var target = GetGameEntityById.Get(owner.targetId.Value);
            var targetPos = owner.targetPoint.Value;
            var targetVel = target.hasVelocity ? target.velocity.Value : Vector3.zero;
            var projSpeed = projectile.movementSpeed.Value;
            var interceptPoint = _targetService.GetInterceptPoint(owner.attackerPoint.Value, projSpeed, targetPos, targetVel);
            var totalDistance = Vector3.Distance(owner.firePoint.Value, interceptPoint);
            var baseArcHeight = projectile.trajectoryBaseArcHeight.Value;
            var distanceFactor = Mathf.Clamp01(totalDistance / 10f);
            var dynamicArcHeight = baseArcHeight * distanceFactor;

            projectile.AddTargetPoint(interceptPoint);
            projectile.AddTotalDistance(totalDistance);
            projectile.ReplaceTrajectoryCurrentArcHeight(dynamicArcHeight);
        }
    }
}