using Code.Game.Features.Ability;
using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class AbilityTargetFilterProperty : EntityProperty
    {
        [field: SerializeField] public AbilityTargetType TargetType { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasAbilityTargetFilter)
                entity.AddAbilityTargetFilter(TargetType);
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasAbilityTargetFilter)
                entity.ReplaceAbilityTargetFilter(TargetType);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasAbilityTargetFilter)
                entity.RemoveAbilityTargetFilter();
        }
    }
}
