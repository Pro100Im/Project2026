using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class MoveSlowingDownProperty : EntityProperty
    {
        [field: SerializeField] public float MoveSlowingDown { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasMoveSlowingDown)
                entity.AddMoveSlowingDown(MoveSlowingDown);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasMoveSlowingDown)
                entity.RemoveMoveSlowingDown();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasMoveSlowingDown)
                entity.ReplaceMoveSlowingDown(MoveSlowingDown);
        }
    }
}