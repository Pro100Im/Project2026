using Code.Game.Common.Entity;
using Code.Game.Features.Target.Services;
using Entitas;

namespace Code.Game.Features.Attack.Systems
{
    public class AttackStartSystem : IExecuteSystem
    {
        private readonly TargetService _targetService;

        private readonly IGroup<GameEntity> _attackers;

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
            foreach (var attacker in _attackers)
            {
                if (!attacker.isAttackAvailable || attacker.isDead || !attacker.hasTargetId)
                    continue;

                var targetId = attacker.targetId.Value;
                var target = GetGameEntityById.Get(targetId);

                if (target.isDead)
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
    }
}