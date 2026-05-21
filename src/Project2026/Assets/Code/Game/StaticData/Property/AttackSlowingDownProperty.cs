using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class AttackSlowingDownProperty : EntityProperty
    {
        [field: SerializeField] public float AttackSlowingDown { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasAttackSlowingDown)
                entity.AddAttackSlowingDown(AttackSlowingDown);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasAttackSlowingDown)
                entity.RemoveAttackSlowingDown();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasAttackSlowingDown)
                entity.ReplaceAttackSlowingDown(AttackSlowingDown);
        }
    }
}