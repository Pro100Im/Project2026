using Code.Game.Common.Entity;
using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Attack.Systems
{
    public class AttackStartSystem : IExecuteSystem
    {
        private readonly TargetService _targetService;

        private readonly IGroup<GameEntity> _attackers;

        private readonly List<GameEntity> _attacksBuffer = new(86);

        public AttackStartSystem(GameContext gameContext, TargetService targetService)
        {
            _targetService = targetService;

            _attackers = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Id,
                    GameMatcher.AttackCooldown,
                    GameMatcher.AttackDuration,
                    GameMatcher.Range,
                    GameMatcher.TargetId));
        }

        public void Execute()
        {
            var attackers = _attackers.GetEntities(_attacksBuffer);

            for (var i = 0; i < attackers.Count; i++)
            {
                var attacker = attackers[i];

                if (!attacker.isAttackAvailable || attacker.isDead || !attacker.hasTargetId)
                    continue;

                var targetId = attacker.targetId.Value;
                var target = GetGameEntityById.Get(targetId);

                if (target.isDead)
                    continue;

                if (attacker.isRangeAttack && !TryFireProjectile(attacker, target))
                    continue;

                var attackDirection = _targetService.GetAttackDirection(attacker.attackerPoint.Value, attacker.targetPoint.Value);

                if (attacker.hasAttackDirection)
                    attacker.ReplaceAttackDirection(attackDirection);
                else
                    attacker.AddAttackDirection(attackDirection);

                attacker.isAttacking = true;
                attacker.isAttackAvailable = false;

                var entity = CreateGameEntity.Empty();

                entity.AddOwnerId(attacker.id.Value);
                entity.AddCooldown(attacker.attackCooldown.Value);
                entity.AddDuration(attacker.attackDuration.Value);

                entity.isMeleeAttack = attacker.isMeleeAttack;
                entity.isRangeAttack = attacker.isRangeAttack;
            }
        }

        private bool TryFireProjectile(GameEntity owner, GameEntity target)
        {
            var projectile = CreateGameEntity.Empty();

            projectile.AddOwnerId(owner.id.Value);
            projectile.AddTargetId(owner.targetId.Value);
            projectile.AddSpawnPosition(owner.firePoint.Value);
            projectile.AddAttackerPoint(owner.firePoint.Value);
            projectile.AddTeam(owner.team.Value);
            projectile.isMovementAvailable = true;

            foreach (var property in owner.projectile.Value.Properties)
                property.Apply(projectile);

            var targetVel = target.hasVelocity ? target.velocity.Value : Vector3.zero;
            var projSpeed = projectile.movementSpeed.Value;
            var interceptPoint = _targetService.GetInterceptPoint(owner.attackerPoint.Value, projSpeed, owner.targetPoint.Value, targetVel);

            var physicalRange = TargetService.GetPhysicalRange(owner.range.Value);
            var dx = interceptPoint.x - owner.attackerPoint.Value.x;
            var dy = interceptPoint.y - owner.attackerPoint.Value.y;

            if ((dx * dx) + (dy * dy) > physicalRange * physicalRange)
            {
                projectile.isDestructed = true;
                return false;
            }

            var totalDistance = Vector3.Distance(owner.firePoint.Value, interceptPoint);
            var baseArcHeight = projectile.trajectoryBaseArcHeight.Value;
            var distanceFactor = Mathf.Clamp01(totalDistance / 10f);
            var dynamicArcHeight = baseArcHeight * distanceFactor;

            projectile.AddTargetPoint(interceptPoint);
            projectile.AddTotalDistance(totalDistance);
            projectile.ReplaceTrajectoryCurrentArcHeight(dynamicArcHeight);

            return true;
        }
    }
}
