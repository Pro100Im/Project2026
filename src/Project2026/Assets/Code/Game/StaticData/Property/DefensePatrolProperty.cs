using Code.Game.StaticData;
using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class DefensePatrolProperty : EntityProperty
    {
        [field: SerializeField] public float IdleDuration { get; private set; } = 3f;

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasDefensePatrolIdleDuration)
                entity.AddDefensePatrolIdleDuration(IdleDuration);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasDefensePatrolIdleDuration)
                entity.RemoveDefensePatrolIdleDuration();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasDefensePatrolIdleDuration)
                entity.ReplaceDefensePatrolIdleDuration(IdleDuration);
        }
    }
}
