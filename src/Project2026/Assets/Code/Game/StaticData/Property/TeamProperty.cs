using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class TeamProperty : EntityProperty
    {
        [field: SerializeField] public Team Team { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasTeam)
                entity.AddTeam(Team);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasTeam)
                entity.RemoveTeam();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasTeam)
                entity.ReplaceTeam(Team);
        }
    }
}