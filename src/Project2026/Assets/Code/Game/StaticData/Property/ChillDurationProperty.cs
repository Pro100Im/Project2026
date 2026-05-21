using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class ChillDurationProperty : EntityProperty
    {
        [field: SerializeField] public float Duration { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasChillDuration)
                entity.AddChillDuration(Duration);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasChillDuration)
                entity.RemoveChillDuration();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasChillDuration)
                entity.ReplaceChillDuration(Duration);
        }
    }
}