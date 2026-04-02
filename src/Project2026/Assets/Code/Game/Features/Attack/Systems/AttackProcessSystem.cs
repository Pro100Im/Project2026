using Code.Common.Time;
using Entitas;

namespace Code.Game.Features.Attack.Systems
{
    public class AttackProcessSystem : IExecuteSystem
    {
        private readonly ITimeService _timeService;
        private readonly IGroup<GameEntity> _attackers;

        public AttackProcessSystem(GameContext gameContext, ITimeService timeService)
        {
            _timeService = timeService;

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
                if (attacker.attackCooldownRemaining.Value > 0)
                {
                    attacker.ReplaceAttackCooldownRemaining(attacker.attackCooldownRemaining.Value - _timeService.DeltaTime);
                }

                if(attacker.attackCooldownRemaining.Value > 0)
                {
                    attacker.ReplaceAttackDurationRemaining(attacker.attackDurationRemaining.Value - _timeService.DeltaTime);
                }
            }
        }
    }
}