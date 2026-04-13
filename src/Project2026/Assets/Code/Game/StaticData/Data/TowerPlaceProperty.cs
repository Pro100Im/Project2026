using System;

namespace Code.Game.StaticData.Data
{
    [Serializable]
    public class TowerPlaceProperty : EntityProperty
    {
        protected override void Add(GameEntity entity)
        {
            entity.isTowerPlace = true;
        }

        protected override void Remove(GameEntity entity)
        {
            entity.isTowerPlace = false;
        }

        protected override void Replace(GameEntity entity)
        {

        }
    }
}