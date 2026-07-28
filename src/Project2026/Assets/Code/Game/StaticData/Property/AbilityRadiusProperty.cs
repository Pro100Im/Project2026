using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class AbilityRadiusProperty : EntityProperty
    {
        [field: SerializeField] public float Radius { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasAbilityRadius)
                entity.AddAbilityRadius(Radius);
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasAbilityRadius)
                entity.ReplaceAbilityRadius(Radius);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasAbilityRadius)
                entity.RemoveAbilityRadius();
        }
    }
}
