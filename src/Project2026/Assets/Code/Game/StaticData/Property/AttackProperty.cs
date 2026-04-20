using System;
using UnityEngine;

namespace Code.Game.StaticData.Data
{
    [Serializable]
    public class AttackProperty : EntityProperty
    {
        [field: SerializeField] public float Range { get; private set; }
        [field: SerializeField] public float Cooldown { get; private set; }
        [field: SerializeField] public float Duration { get; private set; }
        [field: SerializeField] public bool IsMelee { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasRange)
                entity.AddRange(Range);

            if (!entity.hasAttackCooldown)
                entity.AddAttackCooldown(Cooldown);

            if (!entity.hasAttackDuration)
                entity.AddAttackDuration(Duration);

            entity.isMeleeAttack = IsMelee;
            entity.isRangeAttack = !IsMelee;
            entity.isAttackAvailable = true;
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasRange)
                entity.RemoveRange();

            if (entity.hasAttackCooldown)
                entity.RemoveAttackCooldown();

            if (entity.hasAttackDuration)
                entity.RemoveAttackDuration();

            entity.isMeleeAttack = false;
            entity.isRangeAttack = false;
            entity.isAttackAvailable = false;
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasRange)
                entity.ReplaceRange(Range);

            if (entity.hasAttackCooldown)
                entity.ReplaceAttackCooldown(Cooldown);

            if (entity.hasAttackDuration)
                entity.ReplaceAttackDuration(Duration);

            entity.isMeleeAttack = IsMelee;
            entity.isRangeAttack = !IsMelee;
        }
    }
}