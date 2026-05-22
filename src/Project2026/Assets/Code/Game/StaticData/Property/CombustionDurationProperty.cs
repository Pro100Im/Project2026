using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class CombustionDurationProperty : EntityProperty
    {
        [field: SerializeField] public float Duration { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasCombustionDuration)
                entity.AddCombustionDuration(Duration);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasCombustionDuration)
                entity.RemoveCombustionDuration();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasCombustionDuration)
                entity.ReplaceCombustionDuration(Duration);
        }
    } 
}