using Code.Game.Features.Effect.Factory;
using Code.Game.StaticData.Configs;
using Code.Game.StaticData.Data;
using Code.Infrastructure.Identifiers;
using UnityEngine;

namespace Code.Game.Common.Entity
{
    public class GameEntityBuilder 
    {
        private readonly GameEntity _entity;
        private readonly EntityConfig _config;

        public GameEntityBuilder(GameEntity entity, EntityConfig config)
        {
            _entity = entity;
            _config = config;
        }

        public GameEntityBuilder WithId(IIdentifierService identifiers)
        {
            _entity.AddId(identifiers.Next());

            return this;
        }

        public GameEntityBuilder WithSpawnPosition(Vector3 spawnPos)
        {
            _entity.AddSpawnPosition(spawnPos);

            return this;
        }

        public GameEntityBuilder WithView()
        {
            var view = _config.GetProperty<ViewData>();

            if (view != null)
                _entity.AddViewPrefab(view.Prefab);

            return this;
        }

        public GameEntityBuilder WithMovement()
        {
            var movement = _config.GetProperty<MovementData>();

            if (movement != null)
            {
                _entity.AddMovementSpeed(movement.Speed);
                _entity.AddMovementCurrentPointIndex(0);
                _entity.isMovementAvailable = true;
            }

            return this;
        }

        public GameEntityBuilder WithAttack()
        {
            var attack = _config.GetProperty<AttackData>();

            if (attack != null)
            {
                _entity.AddAttackHitEffect(attack.AttackHitEffect);
                _entity.AddDamage(attack.Damage);
                _entity.AddRange(attack.Range);
                _entity.AddAttackCooldown(attack.Cooldown);
                _entity.AddAttackDuration(attack.Duration);
                _entity.isAttackAvailable = true;
            }

            return this;
        }

        public GameEntityBuilder WithHealth(Vector3 spawnPos = default)
        {
            var health = _config.GetProperty<HealthData>();

            if (health != null)
            {
                _entity.AddMaxHealth(health.Health);
                _entity.AddCurrentHealth(health.Health);

                var position = spawnPos == default ? _entity.transform.Value.position : spawnPos;
                var hpBar = CreateGameEntity.Empty();

                hpBar.AddViewPrefab(health.HpBar);
                hpBar.AddSpawnPosition(position + health.HpBarOffset);
                hpBar.AddMovementOffset(health.HpBarOffset);
                hpBar.AddOwnerId(_entity.id.Value);
                hpBar.AddTargetId(_entity.id.Value);
                hpBar.AddCurrentHealth(_entity.currentHealth.Value);
                hpBar.isAttached = true;
            }

            return this;
        }

        public GameEntityBuilder WithSpawnEffect(EffectFactory effectFactory, Vector3 spawnPos = default)
        {
            var position = spawnPos == default ? _entity.transform.Value.position : spawnPos;
            var spawnEffect = _config.GetProperty<SpawnEffectData>();

            if (spawnEffect != null)
                effectFactory.Create(spawnEffect.SpawnEffect, position);

            return this;
        }

        public GameEntity Build()
        {
            return _entity;
        }
    }
}