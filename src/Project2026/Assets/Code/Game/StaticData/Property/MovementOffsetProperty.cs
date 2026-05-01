using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class MovementOffsetProperty : EntityProperty
    {
        [field: SerializeField] public Vector3 Offset { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if(!entity.hasMovementOffset)
                entity.AddMovementOffset(Offset);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasMovementOffset)
                entity.RemoveMovementOffset();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasMovementOffset)
                entity.ReplaceMovementOffset(Offset);
        }
    }
}