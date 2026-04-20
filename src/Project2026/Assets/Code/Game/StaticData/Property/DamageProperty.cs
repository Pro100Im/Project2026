using System;
using UnityEngine;

namespace Code.Game.StaticData.Data
{
    [Serializable]
    public class DamageProperty : EntityProperty
    {
        [field: SerializeField] public float Damage { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasDamage)
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