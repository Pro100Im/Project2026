using Code.Game.StaticData.Configs;
using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class FireAttackProperty : EntityProperty
    {
        [field: SerializeField] public float Damage { get; private set; }
        [field: SerializeField] public EntityConfig HitEffect { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasFireDamageHitEffect)
                entity.AddFireDamageHitEffect(HitEffect);

            if (!entity.hasFireDamage)
                entity.AddFireDamage(Damage);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasFireDamageHitEffect)
                entity.RemoveFireDamageHitEffect();

            if (entity.hasFireDamage)
                entity.RemoveFireDamage();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasFireDamageHitEffect)
                entity.ReplaceFireDamageHitEffect(HitEffect);

            if (entity.hasFireDamage)
                entity.ReplaceFireDamage(Damage);
        }
    }
}