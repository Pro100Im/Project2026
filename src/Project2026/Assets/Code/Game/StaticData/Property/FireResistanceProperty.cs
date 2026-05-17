using Code.Game.StaticData;
using System;
using UnityEngine;

namespace Assets.Code.Game.StaticData.Property
{
    [Serializable]
    public class FireResistanceProperty : EntityProperty
    {
        [field: SerializeField] public int Resistance { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasFireResistance)
                entity.AddFireResistance(Resistance);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasFireResistance)
                entity.RemoveFireResistance();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasFireResistance)
                entity.ReplaceFireResistance(Resistance);
        }
    }
}