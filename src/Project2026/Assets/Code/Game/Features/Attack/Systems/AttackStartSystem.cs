using Code.Game.Common.Entity;
using Entitas;
using UnityEngine;

namespace Code.Game.Features.Attack.Systems
{
    public class AttackStartSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _attackers;

        public AttackStartSystem(GameContext gameContext)
        {
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
            foreach (var attacker in _attackers)
            {
                if (!attacker.isAttackAvailable)
                    continue;

                var targetId = attacker.targetId.Value;
                var target = GetGameEntityById.Get(targetId);

                if (target == null || target.isDead)
                    continue;

                var attackDirection = GetAttackDirection(attacker.attackerPoint.Value, attacker.targetPoint.Value);

                if (attacker.hasAttackDirection)
                    attacker.ReplaceAttackDirection(attackDirection);
                else
                    attacker.AddAttackDirection(attackDirection);

                attacker.isAttacking = true;
                attacker.isAttackAvailable = false;

                var entity = CreateGameEntity.Empty();

                entity.AddPhysicalAttackHitEffect(attacker.physicalAttackHitEffect.Value);
                entity.AddOwnerId(attacker.id.Value);
                entity.AddTargetId(targetId);
                entity.AddCooldown(attacker.attackCooldown.Value);
                entity.AddDuration(attacker.attackDuration.Value);
            }
        }

        private AttackDirection GetAttackDirection(Vector2 closestA, Vector2 closestB)
        {
            var dir = closestB - closestA;

            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                return AttackDirection.Side;
            else
                return dir.y > 0 ? AttackDirection.Up : AttackDirection.Down;
        }
    }
}