using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class FreezeDurationProperty : EntityProperty
    {
        [field: SerializeField] public float Duration { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasFreezeDuration)
                entity.AddFreezeDuration(Duration);
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasFreezeDuration)
                entity.ReplaceFreezeDuration(Duration);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasFreezeDuration)
                entity.RemoveFreezeDuration();
        }
    }
}
