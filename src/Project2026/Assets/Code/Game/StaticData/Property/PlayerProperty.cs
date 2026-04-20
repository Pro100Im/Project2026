using System;

namespace Code.Game.StaticData.Data
{
    [Serializable]
    public class PlayerProperty : EntityProperty
    {
        protected override void Add(GameEntity entity)
        {
            entity.isPlayer = true;
        }

        protected override void Remove(GameEntity entity)
        {
            entity.isPlayer = false;
        }

        protected override void Replace(GameEntity entity)
        {

        }
    }
}