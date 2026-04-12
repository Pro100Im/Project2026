using Code.Infrastructure.View;
using System;
using UnityEngine;

namespace Code.Game.StaticData.Data
{
    [Serializable]
    public class PhysicalDamageProperty : EntityProperty
    {
        //[field: SerializeField] public EntityBehaviour AttackHitEffect { get; private set; }
        [field: SerializeField] public float Damage { get; private set; }
        //[field: SerializeField] public float Range { get; private set; }
        //[field: SerializeField] public float Cooldown { get; private set; }
        //[field: SerializeField] public float Duration { get; private set; }
        //[field: SerializeField] public bool  IsMelee { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if(!entity.hasDamage)
                entity.AddDamage(Damage);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasDamage)
                entity.RemoveDamage();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasDamage)
                entity.ReplaceDamage(Damage);
        }
    }
}