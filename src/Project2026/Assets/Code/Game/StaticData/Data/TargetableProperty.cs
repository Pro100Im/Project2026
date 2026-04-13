using System;

namespace Code.Game.StaticData.Data
{
    [Serializable]
    public class TargetableProperty : EntityProperty
    {
        protected override void Add(GameEntity entity)
        {
            entity.isTargetable = true;
        }

        protected override void Remove(GameEntity entity)
        {
            entity.isTargetable = false;
        }

        protected override void Replace(GameEntity entity)
        {
            
        }
    }
}