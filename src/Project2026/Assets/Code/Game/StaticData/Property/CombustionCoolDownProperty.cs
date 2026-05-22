using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class CombustionCoolDownProperty : EntityProperty
    {
        [field: SerializeField] public float CoolDown { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasCombustionCoolDown)
                entity.AddCombustionCoolDown(CoolDown);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasCombustionCoolDown)
                entity.RemoveCombustionCoolDown();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasCombustionCoolDown)
                entity.ReplaceCombustionCoolDown(CoolDown);
        }
    }
}