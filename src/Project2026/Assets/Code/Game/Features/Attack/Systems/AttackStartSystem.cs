using Code.Common.Entity;
using Entitas;

namespace Code.Game.Features.Attack.Systems
{
    public class AttackStartSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _attackers;

        public AttackStartSystem(GameContext gameContext)
        {
            _attackers = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.AttackCooldown,
                    GameMatcher.TargetId));
        }

        public void Execute()
        {
            foreach (var attacker in _attackers)
            {
                if(!attacker.isAttackAvailable) 
                    continue;

                var targetId = attacker.targetId.Value;
                var target = GetGameEntityById.Get(targetId);

                if (target == null || target.isDead)
                    continue;

                attacker.isAttacking = true;
                attacker.isAttackAvailable = false;

                var entity = CreateGameEntity.Empty();

                entity.AddDamage(attacker.damage.Value);
                entity.AddOwnerId(attacker.id.Value);
                entity.AddTargetId(targetId);
                entity.AddCooldown(attacker.attackCooldown.Value);
                entity.AddDuration(attacker.attackDuration.Value);
            }
        }
    }
}