using Code.Game.StaticData.Configs;
using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class ChillEffectProperty : EntityProperty
    {
        [field: SerializeField] public EntityConfig Config { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasChillEffect)
                entity.AddChillEffect(Config);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasChillEffect)
                entity.RemoveChillEffect();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasChillEffect)
                entity.ReplaceChillEffect(Config);
        }
    }
}