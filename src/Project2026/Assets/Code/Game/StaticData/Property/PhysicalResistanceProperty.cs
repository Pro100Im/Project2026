using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class PhysicalResistanceProperty : EntityProperty
    {
        [field: SerializeField] public int Resistance { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasPhysicalResistance)
                entity.AddPhysicalResistance(Resistance);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasPhysicalResistance)
                entity.RemovePhysicalResistance();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasPhysicalResistance)
                entity.ReplacePhysicalResistance(Resistance);
        }
    }
}