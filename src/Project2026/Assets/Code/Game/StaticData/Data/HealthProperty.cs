using Code.Infrastructure.View;
using System;
using UnityEngine;

namespace Code.Game.StaticData.Data
{
    [Serializable]
    public class HealthProperty : EntityProperty
    {
        //[field: SerializeField] public EntityBehaviour HpBar { get; private set; }
        //[field: SerializeField] public Vector3 HpBarOffset { get; private set; }
        [field: SerializeField] public float Health { get; private set; }
        //[field: SerializeField] public float Regen { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if(!entity.hasMaxHealth)
                entity.AddMaxHealth(Health);

            if(!entity.hasCurrentHealth)
                entity.AddCurrentHealth(Health);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasMaxHealth)
                entity.RemoveMaxHealth();

            if (entity.hasCurrentHealth)
                entity.RemoveCurrentHealth();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasMaxHealth)
                entity.ReplaceMaxHealth(Health);

            if (entity.hasCurrentHealth)
                entity.ReplaceCurrentHealth(Health);
        }
    }
}