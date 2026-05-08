using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Attack.Systems
{
    public class MeleeAttackEndSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _attacks;
        private readonly List<GameEntity> _buffer = new(64);

        public MeleeAttackEndSystem(GameContext gameContext)
        {
            _attacks = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.OwnerId,
                    GameMatcher.Cooldown,
                    GameMatcher.Duration,
                    GameMatcher.MeleeAttack));
        }

        public void Execute()
        {
            foreach (var attack in _attacks.GetEntities(_buffer))
            {
                if (attack.duration.Value > 0)
                    continue;

                var entity = GetGameEntityById.Get(attack.ownerId.Value);

                if(!entity.hasTargetId)
                {
                    entity.isAttacking = false;
                    entity.isAttackAvailable = true;

                    attack.isDestructed = true;

                    continue;
                }

                if(entity.isAttacking)
                {
                    var damage = CreateGameEntity.Empty();

                    damage.AddOwnerId(entity.id.Value);
                    damage.AddTargetId(entity.targetId.Value);
                    damage.AddTotalDamage(0);
                    damage.isDamageRequest = true;
                }

                entity.isAttacking = false;

                if (attack.cooldown.Value > 0)
                    continue;

                entity.isAttackAvailable = true;
                attack.isDestructed = true;
            }
        }
    }
}