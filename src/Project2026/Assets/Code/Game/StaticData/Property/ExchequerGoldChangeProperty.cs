using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class ExchequerGoldChangeProperty : EntityProperty
    {
        [field: SerializeField] public int Value { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasExchequerGoldChangeRequest)
                entity.AddExchequerGoldChangeRequest(Value);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasExchequerGoldChangeRequest)
                entity.RemoveExchequerGoldChangeRequest();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasExchequerGoldChangeRequest)
                entity.ReplaceExchequerGoldChangeRequest(Value);
        }
    }
}