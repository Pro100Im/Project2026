using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class FrostResistanceProperty : EntityProperty
    {
        [field: SerializeField] public int Resistance { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasFrostResistance)
                entity.AddFrostResistance(Resistance);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasFrostResistance)
                entity.RemoveFrostResistance();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasFrostResistance)
                entity.ReplaceFrostResistance(Resistance);
        }
    }
}