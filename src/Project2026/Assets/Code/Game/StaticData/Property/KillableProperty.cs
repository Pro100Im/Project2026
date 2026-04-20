using System;

namespace Code.Game.StaticData.Data
{
    [Serializable]
    public class KillableProperty : EntityProperty
    {
        protected override void Add(GameEntity entity)
        {
            entity.isKillable = true;
        }

        protected override void Remove(GameEntity entity)
        {
            entity.isKillable = false;
        }

        protected override void Replace(GameEntity entity)
        {

        }
    }
}