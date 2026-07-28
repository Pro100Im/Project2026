using System;

namespace Code.Game.StaticData.Data
{
    [Serializable]
    public class TowerPlaceProperty : EntityProperty
    {
        protected override void Add(GameEntity entity)
        {
            entity.isTowerPlace = true;
            entity.isInteractable = true;
        }

        protected override void Remove(GameEntity entity)
        {
            entity.isTowerPlace = false;
            entity.isInteractable = false;
        }

        protected override void Replace(GameEntity entity)
        {

        }
    }
}