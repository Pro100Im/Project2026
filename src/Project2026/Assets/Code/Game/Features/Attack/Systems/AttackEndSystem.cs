using Code.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Attack.Systems
{
    public class AttackEndSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _attacks;
        private readonly List<GameEntity> _buffer = new(64);

        public AttackEndSystem(GameContext gameContext)
        {
            _attacks = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Damage,
                    GameMatcher.OwnerId,
                    GameMatcher.TargetId,
                    GameMatcher.Cooldown,
                    GameMatcher.Duration));
        }

        public void Execute()
        {
            foreach (var attack in _attacks.GetEntities(_buffer))
            {
                if (attack.duration.Value > 0)
                    continue;

                var entity = GetGameEntityById.Get(attack.ownerId.Value);

                entity.isAttacking = false;

                if (attack.cooldown.Value > 0)
                    continue;

                entity.isAttackAvailable = true;

                attack.Destroy();

                //var entity = CreateGameEntity.Empty();

                //entity.isDamageRequest = true;
                //entity.AddTargetId(attacker.targetId.Value);
                //entity.AddDamage(attacker.attack.Value);
            }
        }
    }
}