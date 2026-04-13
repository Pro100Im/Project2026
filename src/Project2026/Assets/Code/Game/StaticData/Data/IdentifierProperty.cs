using Code.Infrastructure.Identifiers;
using System;

namespace Code.Game.StaticData.Data
{
    [Serializable]
    public class IdentifierProperty : EntityProperty
    {
        protected override void Add(GameEntity entity)
        {
            if (!entity.hasId)
                entity.AddId(EntityIdentifier.Next());
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasId)
                entity.RemoveId();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasId)
                entity.ReplaceId(EntityIdentifier.Next());
        }
    }
}