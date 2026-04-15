using System;
using UnityEngine;

namespace Code.Game.StaticData.Data
{
    [Serializable]
    public class DeathDurationProperty : EntityProperty
    {
        [field: SerializeField] public float Duration { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasDeathDuration)
                entity.AddDeathDuration(Duration);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasDeathDuration)
                entity.RemoveDeathDuration();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasDeathDuration)
                entity.ReplaceDeathDuration(Duration);
        }
    }
}