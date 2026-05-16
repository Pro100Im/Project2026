using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class AreaAttackProperty : EntityProperty
    {
        [field: SerializeField] public float Area { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasAreaAttack)
                entity.AddAreaAttack(Area);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasAreaAttack)
                entity.RemoveAreaAttack();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasAreaAttack)
                entity.ReplaceAreaAttack(Area);
        }
    }
}