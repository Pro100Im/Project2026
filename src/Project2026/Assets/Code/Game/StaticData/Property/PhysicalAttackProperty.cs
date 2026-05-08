using Code.Game.StaticData.Configs;
using System;
using UnityEngine;

namespace Code.Game.StaticData.Data
{
    [Serializable]
    public class PhysicalAttackProperty : EntityProperty
    {
        [field: SerializeField] public float PhysicalDamage { get; private set; }
        [field: SerializeField] public EntityConfig PhysicalHitEffect { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasPhysicalDamageHitEffect)
                entity.AddPhysicalDamageHitEffect(PhysicalHitEffect);

            if(!entity.hasPhysicalDamage)
                entity.AddPhysicalDamage(PhysicalDamage);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasPhysicalDamageHitEffect)
                entity.RemovePhysicalDamageHitEffect();

            if (entity.hasPhysicalDamage)
                entity.RemovePhysicalDamage();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasPhysicalDamageHitEffect)
                entity.ReplacePhysicalDamageHitEffect(PhysicalHitEffect);

            if (entity.hasPhysicalDamage)
                entity.ReplacePhysicalDamage(PhysicalDamage);
        }
    }
}