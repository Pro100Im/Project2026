using Code.Game.StaticData.Configs;
using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class RewardProperty : EntityProperty
    {
        [field: SerializeField] public EntityConfig Reward { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasReward)
                entity.AddReward(Reward);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasReward)
                entity.RemoveReward();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasReward)
                entity.ReplaceReward(Reward);
        }
    }
}