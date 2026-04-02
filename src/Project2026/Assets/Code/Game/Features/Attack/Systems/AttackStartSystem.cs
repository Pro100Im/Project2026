using Entitas;

namespace Code.Game.Features.Attack.Systems
{
    public class AttackStartSystem : IExecuteSystem
    {
        private readonly GameContext _gameContext;
        private readonly IGroup<GameEntity> _attackers;

        public AttackStartSystem(GameContext gameContext)
        {
            _gameContext = gameContext;

            _attackers = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Attack,
                    GameMatcher.AttackCooldown,
                    GameMatcher.AttackCooldownRemaining,
                    GameMatcher.TargetId));
        }

        public void Execute()
        {
            foreach (var attacker in _attackers)
            {
                if(attacker.isAttacking || attacker.attackCooldownRemaining.Value > 0)
                    continue;

                var targetId = attacker.targetId.Value;
                var target = _gameContext.GetEntityWithId(targetId);

                if (target == null || target.isDead)
                    continue;

                attacker.ReplaceAttackCooldownRemaining(attacker.attackCooldown.Value);
                attacker.ReplaceAttackDurationRemaining(attacker.attackDuration.Value);

                attacker.isAttacking = true;
            }
        }
    }
}