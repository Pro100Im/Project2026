using Code.Game.StaticData.Configs;
using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class FrostAttackProperty : EntityProperty
    {
        [field: SerializeField] public float Damage { get; private set; }
        [field: SerializeField] public EntityConfig HitEffect { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasFrostDamageHitEffect)
                entity.AddFrostDamageHitEffect(HitEffect);

            if (!entity.hasFrostDamage)
                entity.AddFrostDamage(Damage);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasFrostDamageHitEffect)
                entity.RemoveFrostDamageHitEffect();

            if (entity.hasFrostDamage)
                entity.RemoveFrostDamage();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasFrostDamageHitEffect)
                entity.ReplaceFrostDamageHitEffect(HitEffect);

            if (entity.hasFrostDamage)
                entity.ReplaceFrostDamage(Damage);
        }
    }
}