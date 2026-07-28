using Code.Game.StaticData.Configs;
using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class FreezeEffectProperty : EntityProperty
    {
        [field: SerializeField] public EntityConfig Config { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasFreezeEffect)
                entity.AddFreezeEffect(Config);
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasFreezeEffect)
                entity.ReplaceFreezeEffect(Config);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasFreezeEffect)
                entity.RemoveFreezeEffect();
        }
    }
}
