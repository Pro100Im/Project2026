using Code.Game.StaticData;
using System;
using UnityEngine;

namespace Assets.Code.Game.StaticData.Property
{
    /// <summary>
    /// Body radius of a unit in world units, used for crowding and wall collision.
    /// Optional: without it the radius is derived from the unit size.
    /// </summary>
    [Serializable]
    public class UnitRadiusProperty : EntityProperty
    {
        [field: SerializeField, Min(0.05f)] public float Radius { get; private set; } = 0.18f;

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasUnitRadius)
                entity.AddUnitRadius(Radius);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasUnitRadius)
                entity.RemoveUnitRadius();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasUnitRadius)
                entity.ReplaceUnitRadius(Radius);
        }
    }
}
