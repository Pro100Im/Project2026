using Code.Game.StaticData;
using System;

namespace Assets.Code.Game.StaticData.Property
{
    [Serializable]
    public class GridMovementProperty : EntityProperty
    {
        protected override void Add(GameEntity entity)
        {
            if (!entity.isGridMovement)
                entity.isGridMovement = true;
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.isGridMovement)
                entity.isGridMovement = false;
        }

        protected override void Replace(GameEntity entity)
        {
            
        }
    }
}