using Code.Common.Entity;
using Code.Game.Features.Health;
using Code.Game.StaticData.Configs;
using Code.Game.StaticData.Data;
using Code.Infrastructure.Identifiers;
using Entitas;
using UnityEngine;

namespace Code.Game.Features.Enemy.Factory
{
    public class EnemyFactory
    {
        private readonly IIdentifierService _identifiers;

        public EnemyFactory(IIdentifierService identifiers)
        {
            _identifiers = identifiers;
        }

        public GameEntity Create(EntityConfig entityConfig, Vector2 spawnPosition)
        {
            var entity = CreateGameEntity.Empty();
            entity.AddId(_identifiers.Next());
            entity.AddSpawnPosition(spawnPosition);
   
            entity.isEnemy = true;
            entity.isMovementAvailable = true;
            entity.isTargetable = true;

            var view = entityConfig.GetProperty<ViewData>();
            if (view != null)
                entity.AddViewPrefab(view.Prefab);

            var movement = entityConfig.GetProperty<MovementData>();
            if (movement != null)
            {
                entity.AddMovementSpeed(movement.Speed);
                entity.AddMovementCurrentPointIndex(0);
                entity.isMovementAvailable = true;
            }

            var attack = entityConfig.GetProperty<AttackData>();
            if (attack != null)
            {
                entity.AddAttackHitEffect(attack.AttackHitEffect);
                entity.AddDamage(attack.Damage);
                entity.AddRange(attack.Range);
                entity.AddAttackCooldown(attack.Cooldown);
                entity.AddAttackDuration(attack.Duration);
                entity.isTargetable = true;
                entity.isAttackAvailable = true;
            }

            var health = entityConfig.GetProperty<HealthData>();
            if (health != null)
            {
                entity.AddMaxHealth(health.Health);
                entity.AddCurrentHealth(health.Health);

                var hpBar = CreateGameEntity.Empty();
                hpBar.AddViewPrefab(health.HpBar);
                hpBar.AddSpawnPosition((Vector3)spawnPosition + health.HpBarOffset);
                hpBar.AddMovementOffset(health.HpBarOffset);
                hpBar.AddOwnerId(entity.id.Value);
                hpBar.AddTargetId(entity.id.Value);
                hpBar.AddCurrentHealth(entity.currentHealth.Value);
                hpBar.isAttached = true;
                hpBar.isEnemy = true;
            }

            var spawnEffect = entityConfig.GetProperty<SpawnEffectData>();
            if (spawnEffect != null)
            {
                var effect = CreateGameEntity.Empty();
                effect.AddId(_identifiers.Next());
                effect.AddViewPrefab(spawnEffect.SpawnEffect);
                effect.AddSpawnPosition(spawnPosition);
            }

                return entity;
        }
    }
}