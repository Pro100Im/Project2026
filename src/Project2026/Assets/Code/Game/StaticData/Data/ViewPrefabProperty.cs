using Code.Infrastructure.View;
using System;
using UnityEngine;

namespace Code.Game.StaticData.Data
{
    [Serializable]
    public class ViewPrefabProperty : EntityProperty
    {
        [field: SerializeField] public EntityBehaviour Prefab { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if(!entity.hasViewPrefab)
                entity.AddViewPrefab(Prefab);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasViewPrefab)
                entity.RemoveViewPrefab();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasViewPrefab)
                entity.ReplaceViewPrefab(Prefab);
        }
    }
}