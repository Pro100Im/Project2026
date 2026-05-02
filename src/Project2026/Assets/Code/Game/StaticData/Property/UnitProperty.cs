using System;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class UnitProperty : EntityProperty
    {
        protected override void Add(GameEntity entity)
        {
            entity.isUnit = true;
        }

        protected override void Remove(GameEntity entity)
        {
            entity.isUnit = false;
        }

        protected override void Replace(GameEntity entity)
        {

        }
    }
}