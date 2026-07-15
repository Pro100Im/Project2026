using Code.Game.Common.Entity;
using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;

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

                if (!attacker.isAttackAvailable || attacker.isAttacking || attacker.isDead || !attacker.hasTargetId)
                    continue;

                var targetId = attacker.targetId.Value;
                var target = GetGameEntityById.Get(targetId);

                if (target == null || target.isDead)
                    continue;

                if (attacker.isMeleeAttack)
                {
                    var physicalRange = TargetService.GetPhysicalRange(attacker.range.Value);
                    var dx = attacker.attackerPoint.Value.x - attacker.targetPoint.Value.x;
                    var dy = attacker.attackerPoint.Value.y - attacker.targetPoint.Value.y;

                    if ((dx * dx) + (dy * dy) > physicalRange * physicalRange)
                        continue;
                }

                if (attacker.isRangeAttack && !AttackProjectileHelper.CanFire(_targetService, attacker, target))
                    continue;

                var attackDirection = _targetService.GetAttackDirection(attacker.attackerPoint.Value, attacker.targetPoint.Value);

                if (attacker.hasAttackDirection)
                    attacker.ReplaceAttackDirection(attackDirection);
                else
                    attacker.AddAttackDirection(attackDirection);

                if (attacker.hasSpriteRenderer)
                {
                    var dx = attacker.targetPoint.Value.x - attacker.attackerPoint.Value.x;

                    if (dx != 0f)
                    {
                        var shouldFlipX = dx < 0f;
                        var spriteRenderer = attacker.spriteRenderer.Value;

                        if (spriteRenderer.flipX != shouldFlipX)
                            spriteRenderer.flipX = shouldFlipX;
                    }
                }

                attacker.isAttacking = true;
                attacker.isAttackAvailable = false;

                if (attacker.isMoving)
                    attacker.isMoving = false;

                if (attacker.hasVelocity)
                    attacker.RemoveVelocity();

                var entity = CreateGameEntity.Empty();

                entity.AddOwnerId(attacker.id.Value);
                entity.AddCooldown(attacker.attackCooldown.Value);
                entity.AddDuration(attacker.attackDuration.Value);

                entity.isMeleeAttack = attacker.isMeleeAttack;
                entity.isRangeAttack = attacker.isRangeAttack;
                entity.isAttackHitPending = true;
            }
        }
    }
}
