using System;

namespace Code.Game.StaticData.Data
{
    [Serializable]
    public class PlayerCastleProperty : EntityProperty
    {
        protected override void Add(GameEntity entity)
        {
            entity.isPlayerCastle = true;
        }

        protected override void Remove(GameEntity entity)
        {
            entity.isPlayerCastle = false;
        }

        protected override void Replace(GameEntity entity)
        {

        }
    }
}