using Entitas;

namespace Code.Game.Features.Attack.Systems
{
    public class AttackEndSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _attackers;

        public AttackEndSystem(GameContext gameContext)
        {
            _attackers = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Attack,
                    GameMatcher.AttackDurationRemaining,
                    GameMatcher.TargetId));
        }

        public void Execute()
        {
            foreach (var attacker in _attackers)
            {
                if (!attacker.isAttacking || attacker.attackDurationRemaining.Value > 0)
                    continue;

                attacker.isAttacking = false;
            }
        }
    }
}