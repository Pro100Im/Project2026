using Code.Game.Common.Entity;
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
                    GameMatcher.PhysicalAttackHitEffect,
                    GameMatcher.OwnerId,
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

                if(!entity.hasTargetId)
                {
                    entity.isAttacking = false;
                    entity.isAttackAvailable = true;

                    attack.isDestructed = true;

                    continue;
                }

                if(entity.isAttacking)
                {
                    var hitEffect = CreateGameEntity.Empty();
                    var targetPoint = entity.targetPoint.Value;
                    var movementOffset = entity.movementOffset.Value;

                    targetPoint.x += movementOffset.x;
                    targetPoint.y += movementOffset.y;

                    hitEffect.AddSpawnPosition(targetPoint);

                    foreach (var property in entity.physicalAttackHitEffect.Value.Properties)
                        property.Apply(hitEffect);

                    var damage = CreateGameEntity.Empty();

                    damage.AddOwnerId(entity.id.Value);
                    damage.AddTargetId(entity.targetId.Value);
                    damage.AddDamage(hitEffect.damage.Value);
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