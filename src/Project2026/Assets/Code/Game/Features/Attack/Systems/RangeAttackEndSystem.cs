using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Attack.Systems
{
    public class RangeAttackEndSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _rangeAttacks;
        private readonly List<GameEntity> _buffer = new(64);

        public RangeAttackEndSystem(GameContext gameContext)
        {
            _rangeAttacks = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.OwnerId,
                    GameMatcher.Cooldown,
                    GameMatcher.Duration,
                    GameMatcher.RangeAttack));
        }

        public void Execute()
        {
            foreach (var attack in _rangeAttacks.GetEntities(_buffer))
            {
                if (attack.duration.Value > 0)
                    continue;

                var entity = GetGameEntityById.Get(attack.ownerId.Value);

                if (!entity.hasTargetId)
                {
                    entity.isAttacking = false;
                    entity.isAttackAvailable = true;

                    attack.isDestructed = true;

                    continue;
                }

                if (entity.isAttacking)
                    CreateProjectile(entity);

                entity.isAttacking = false;

                if (attack.cooldown.Value > 0)
                    continue;

                entity.isAttackAvailable = true;
                attack.isDestructed = true;
            }
        }

        private void CreateProjectile(GameEntity owner)
        {
            var projectile = CreateGameEntity.Empty();

            projectile.AddOwnerId(owner.id.Value);
            projectile.AddSpawnPosition(owner.firePoint.Value);
            projectile.AddAttackerPoint(owner.attackerPoint.Value);
            projectile.AddTargetPoint(owner.targetPoint.Value);
            projectile.isMovementAvailable = true;

            foreach (var property in owner.projectile.Value.Properties)
                property.Apply(projectile);
        }
    }
}