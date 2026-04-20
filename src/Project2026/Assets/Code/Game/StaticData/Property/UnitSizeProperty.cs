using Code.Game.StaticData;
using System;
using UnityEngine;

namespace Assets.Code.Game.StaticData.Property
{
    [Serializable]
    public class UnitSizeProperty : EntityProperty
    {
        [field: SerializeField] public Vector2Int Size { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasUnitSize)
                entity.AddUnitSize(Size);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasUnitSize)
                entity.RemoveUnitSize();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasUnitSize)
                entity.ReplaceUnitSize(Size);
        }
    }
}