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
        private readonly IGroup<GameEntity> _maps;

        private readonly List<GameEntity> _attacksBuffer = new(86);

        public AttackStartSystem(TargetService targetService)
        {
            var gameContext = Contexts.sharedInstance.game;

            _targetService = targetService;

            _attackers = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Id,
                    GameMatcher.AttackCooldown,
                    GameMatcher.AttackDuration,
                    GameMatcher.Range,
                    GameMatcher.TargetId,
                    GameMatcher.CurrentCell,
                    GameMatcher.UnitSize));

            _maps = gameContext.GetGroup(GameMatcher.AllOf(GameMatcher.TilemapMovement));
        }

        public void Execute()
        {
            var mapEntity = _maps.GetSingleEntity();

            if (mapEntity == null)
                return;

            var tilemap = mapEntity.tilemapMovement.Value;
            var attackers = _attackers.GetEntities(_attacksBuffer);

            for (var i = 0; i < attackers.Count; i++)
            {
                var attacker = attackers[i];

                if (!attacker.isAttackAvailable
                    || attacker.isAttacking
                    || attacker.isDead
                    || attacker.isFreezeDebuff
                    || !attacker.hasTargetId)
                    continue;

                var targetId = attacker.targetId.Value;
                var target = GetGameEntityById.Get(targetId);

                if (target == null || target.isDead || !target.hasCurrentCell)
                    continue;

                if (!IsOnAttackRing(attacker, target))
                    continue;

                if (attacker.isRangeAttack)
                {
                    var isRetreating = attacker.hasSurroundSlot
                        && attacker.currentCell.Value != attacker.surroundSlot.Value;

                    if (isRetreating && attacker.isMoving
                        && TargetService.IsTooCloseForRanged(attacker, target, tilemap))
                        continue;

                    if (!AttackProjectileHelper.CanFire(_targetService, attacker, target))
                        continue;
                }

                ResolveAttackFacing(attacker, target, out var attackDirection, out var flipDx);

                if (attacker.hasAttackDirection)
                    attacker.ReplaceAttackDirection(attackDirection);
                else
                    attacker.AddAttackDirection(attackDirection);

                if (attacker.hasSpriteRenderer && flipDx != 0f)
                {
                    var shouldFlipX = flipDx < 0f;
                    var spriteRenderer = attacker.spriteRenderer.Value;

                    if (spriteRenderer.flipX != shouldFlipX)
                        spriteRenderer.flipX = shouldFlipX;
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

        private void ResolveAttackFacing(GameEntity attacker, GameEntity target, out AttackDirection attackDirection, out float flipDx)
        {
            var worldDx = attacker.targetPoint.Value.x - attacker.attackerPoint.Value.x;
            var worldDy = attacker.targetPoint.Value.y - attacker.attackerPoint.Value.y;

            if ((worldDx * worldDx) + (worldDy * worldDy) >= 1e-6f)
            {
                attackDirection = _targetService.GetAttackDirection(
                    attacker.attackerPoint.Value,
                    attacker.targetPoint.Value);
                flipDx = worldDx;
                return;
            }

            TargetService.GetFootprint(target, out var minX, out var minY, out var maxX, out var maxY);

            var cell = attacker.currentCell.Value;
            var dx = Mathf.Clamp(cell.x, minX, maxX) - cell.x;
            var dy = Mathf.Clamp(cell.y, minY, maxY) - cell.y;

            if (Mathf.Abs(dx) > Mathf.Abs(dy))
                attackDirection = AttackDirection.Side;
            else
                attackDirection = dy > 0 ? AttackDirection.Up : AttackDirection.Down;

            flipDx = dx;
        }

        private bool IsOnAttackRing(GameEntity attacker, GameEntity target) =>
            TargetService.IsOnAttackRing(attacker, target);
    }
}
