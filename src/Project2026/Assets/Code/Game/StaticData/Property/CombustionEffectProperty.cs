using Code.Game.StaticData.Configs;
using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class CombustionEffectProperty : EntityProperty
    {
        [field: SerializeField] public EntityConfig Config { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasCombustionEffect)
                entity.AddCombustionEffect(Config);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasCombustionEffect)
                entity.RemoveCombustionEffect();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasCombustionEffect)
                entity.ReplaceCombustionEffect(Config);
        }
    }
}