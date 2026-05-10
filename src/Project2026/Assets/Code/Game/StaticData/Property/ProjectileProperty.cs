using Code.Game.StaticData.Configs;
using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class ProjectileProperty : EntityProperty
    {
        [field: SerializeField] public EntityConfig Projectile { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.hasProjectile)
                entity.AddProjectile(Projectile);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.hasProjectile)
                entity.RemoveProjectile();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasProjectile)
                entity.ReplaceProjectile(Projectile);
        }
    }
}