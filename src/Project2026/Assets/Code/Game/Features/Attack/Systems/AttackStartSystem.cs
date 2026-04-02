using Entitas;

namespace Code.Game.Features.Attack.Systems
{
    public class AttackStartSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _attackers;
        private readonly GameContext _gameContext;

        public AttackStartSystem(GameContext gameContext)
        {
            _attackers = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Attack,
                    GameMatcher.TargetId));

            _gameContext = gameContext;
        }

        public void Execute()
        {
            foreach (var attacker in _attackers)
            {
                if(attacker.isAttacking)
                    continue;

                var targetId = attacker.targetId.Value;
                var target = _gameContext.GetEntityWithId(targetId);

                if (target == null || target.isDead)
                    continue;

                attacker.isAttacking = true;
            }
        }
    }
}