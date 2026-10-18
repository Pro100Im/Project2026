using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class PassiveBlockProperty : EntityProperty
    {
        [field: SerializeField] public float Chance { get; private set; }
        [field: SerializeField] public float Duration { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasPassiveBlockChance)
                entity.AddPassiveBlockChance(Chance);

            if (!entity.hasPassiveBlockDuration)
                entity.AddPassiveBlockDuration(Duration);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasPassiveBlockChance)
                entity.RemovePassiveBlockChance();

            if (entity.hasPassiveBlockDuration)
                entity.RemovePassiveBlockDuration();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasPassiveBlockChance)
                entity.ReplacePassiveBlockChance(Chance);

            if (entity.hasPassiveBlockDuration)
                entity.ReplacePassiveBlockDuration(Duration);
        }
    }
}
