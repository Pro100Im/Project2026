using Code.Game.Common.Entity;
using Code.Game.StaticData.Configs;
using System;
using UnityEngine;

namespace Code.Game.StaticData.Data
{
    [Serializable]
    public class HealthBarProperty : EntityProperty
    {
        [field: SerializeField] public EntityConfig HpBar { get; private set; }
        [field: SerializeField] public Vector3 SpawnOffset { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (entity.hasMaxHealth && entity.hasCurrentHealth && !entity.hasHpBar)
            {
                var hpBarEntity = CreateGameEntity.Empty();
                var spawnPos = entity.hasSpawnPosition ? entity.spawnPosition.Value + SpawnOffset : entity.transform.Value.position + SpawnOffset;

                hpBarEntity.AddSpawnPosition(spawnPos);
                hpBarEntity.AddTargetId(entity.id.Value);
                hpBarEntity.AddOwnerId(entity.id.Value);
                hpBarEntity.AddMovementOffset(SpawnOffset);
                hpBarEntity.AddCurrentHealth(entity.currentHealth.Value);
                hpBarEntity.isAttached = true;

                foreach (var property in HpBar.Properties)
                    property.Apply(hpBarEntity);
            }
        }

        protected override void Remove(GameEntity entity)
        {
            
        }

        protected override void Replace(GameEntity entity)
        {
            
        }
    }
}