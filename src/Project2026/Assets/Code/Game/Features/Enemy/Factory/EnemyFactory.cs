using Code.Common.Entity;
using Code.Game.StaticData.Configs;
using Code.Game.StaticData.Data;
using Code.Infrastructure.Identifiers;
using UnityEngine;

namespace Code.Game.Features.Enemy.Factory
{
    public class EnemyFactory
    {
        private readonly IIdentifierService _identifiers;

        public EnemyFactory(IIdentifierService identifiers)
        {
            _identifiers = identifiers;
        }

        public GameEntity Create(EntityConfig entityConfig, Vector2 spawnPosition)
        {
            var entity = CreateGameEntity.Empty();
            entity.AddId(_identifiers.Next());
            entity.AddSpawnPosition(spawnPosition);
   
            entity.isEnemy = true;
            entity.isMovementAvailable = true;

            var view = entityConfig.GetProperty<ViewData>();
            if (view != null)
                entity.AddViewPrefab(view.Prefab);

            var movement = entityConfig.GetProperty<MovementData>();
            if (movement != null)
            {
                entity.AddMovementSpeed(movement.Speed);
                entity.AddMovementCurrentPointIndex(0);
                entity.isMovementAvailable = true;
            }

            return entity;
        }
    }
}