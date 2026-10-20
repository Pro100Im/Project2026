using Code.Game.StaticData;
using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class UnitKindProperty : EntityProperty
    {
        [field: SerializeField] public UnitKind Kind { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasUnitKind)
                entity.AddUnitKind(Kind);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasUnitKind)
                entity.RemoveUnitKind();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasUnitKind)
                entity.ReplaceUnitKind(Kind);
        }
    }
}
