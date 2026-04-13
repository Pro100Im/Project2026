using Code.Game.Common.Entity;
using Code.Game.StaticData.Configs;
using System;
using UnityEngine;

namespace Code.Game.StaticData.Data
{
    [Serializable]
    public class SpawnEffectProperty : EntityProperty
    {
        [field: SerializeField] public EntityConfig SpawnEffect { get; private set; }
        [field: SerializeField] public Vector3 SpawnEffectOffset { get; private set; }

        protected override void Add(GameEntity entity)
        {
            var spawnEffectEntity = CreateGameEntity.Empty();
            var spawnPos = entity.hasSpawnPosition ? entity.spawnPosition.Value + SpawnEffectOffset : entity.transform.Value.position + SpawnEffectOffset;

            spawnEffectEntity.AddSpawnPosition(spawnPos);

            foreach (var property in SpawnEffect.Properties)
                property.Apply(spawnEffectEntity);
        }

        protected override void Remove(GameEntity entity)
        {
            // To do
        }

        protected override void Replace(GameEntity entity)
        {
            // To do
        }
    }
}