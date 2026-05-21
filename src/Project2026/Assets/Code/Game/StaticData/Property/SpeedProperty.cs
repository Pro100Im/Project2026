using System;
using UnityEngine;

namespace Code.Game.StaticData.Data
{
    [Serializable]
    public class SpeedProperty : EntityProperty
    {
        [field: SerializeField] public float Speed { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if(!entity.hasMovementSpeed)
                entity.AddMovementSpeed(Speed);

            if(!entity.hasMovementSpeedBonus)
                entity.AddMovementSpeedBonus(1f);
        }

        protected override void Remove(GameEntity entity)
        {
            if(entity.hasMovementSpeed)
                entity.RemoveMovementSpeed();

            if(entity.hasMovementSpeedBonus)
                entity.RemoveMovementSpeedBonus();
        }

        protected override void Replace(GameEntity entity)
        {
            if(entity.hasMovementSpeed)
                entity.ReplaceMovementSpeed(Speed);
        }
    }
}