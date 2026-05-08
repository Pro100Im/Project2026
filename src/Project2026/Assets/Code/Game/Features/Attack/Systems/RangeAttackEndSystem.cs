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

                var owner = GetGameEntityById.Get(attack.ownerId.Value);

                if (owner == null) 
                { 
                    attack.isDestructed = true; 

                    continue; 
                }

                if (owner.hasTargetId && owner.isAttacking && owner.hasProjectileConfig)
                    CreateProjectile(owner);

                owner.isAttacking = false;
                owner.isAttackAvailable = true;

                attack.isDestructed = true;
            }
        }

        private void CreateProjectile(GameEntity owner)
        {
            var projectile = CreateGameEntity.Empty();

            projectile.AddSpawnPosition(owner.firePoint.Value);

            foreach (var property in owner.projectileConfig.Value.Properties)
                property.Apply(projectile);
        }
    }
}