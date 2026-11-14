using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class TownGoldRewardProperty : EntityProperty
    {
        [field: SerializeField] public int Value { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasTownGoldReward)
                entity.AddTownGoldReward(Value);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasTownGoldReward)
                entity.RemoveTownGoldReward();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasTownGoldReward)
                entity.ReplaceTownGoldReward(Value);
        }
    }
}
