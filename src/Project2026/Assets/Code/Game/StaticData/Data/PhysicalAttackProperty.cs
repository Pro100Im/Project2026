using Code.Game.StaticData.Configs;
using System;
using UnityEngine;

namespace Code.Game.StaticData.Data
{
    [Serializable]
    public class PhysicalAttackProperty : EntityProperty
    {
        [field: SerializeField] public EntityConfig AttackHitEffect { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasPhysicalAttackHitEffect)
                entity.AddPhysicalAttackHitEffect(AttackHitEffect);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasPhysicalAttackHitEffect)
                entity.RemovePhysicalAttackHitEffect();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasPhysicalAttackHitEffect)
                entity.ReplacePhysicalAttackHitEffect(AttackHitEffect);
        }
    }
}