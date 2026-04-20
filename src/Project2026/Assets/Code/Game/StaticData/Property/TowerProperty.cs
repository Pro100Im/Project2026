using System;

namespace Code.Game.StaticData.Data
{
    [Serializable]
    public class TowerProperty : EntityProperty
    {
        protected override void Add(GameEntity entity)
        {
            entity.isTower = true;
        }

        protected override void Remove(GameEntity entity)
        {
            entity.isTower = false;
        }

        protected override void Replace(GameEntity entity)
        {

        }
    }
}