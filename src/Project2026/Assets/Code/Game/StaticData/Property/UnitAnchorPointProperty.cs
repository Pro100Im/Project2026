using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class UnitAnchorPointProperty : EntityProperty
    {
        [field: SerializeField] public Vector3 AnchorPoint { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasUnitAnchorPoint)
                entity.AddUnitAnchorPoint(AnchorPoint);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasUnitAnchorPoint)
                entity.RemoveUnitAnchorPoint();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasUnitAnchorPoint)
                entity.ReplaceUnitAnchorPoint(AnchorPoint);
        }
    }
}