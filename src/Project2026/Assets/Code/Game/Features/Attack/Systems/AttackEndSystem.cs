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

                if(entity.isAttacking)
                {
                    var damage = CreateGameEntity.Empty();

                    damage.AddOwnerId(attack.ownerId.Value);
                    damage.AddTargetId(attack.targetId.Value);
                    damage.AddDamage(attack.damage.Value);
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