using Code.Common.Entity;
using Code.Game.StaticData.Configs;
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

        public GameEntity Create(EntityConfig entityConfig, Vector2 spawnPosition, int sortOrder)
        {
            var entity = CreateGameEntity.Empty();
            entity.AddId(_identifiers.Next());
            entity.AddSpawnPosition(spawnPosition);
            entity.AddSortOrder(sortOrder);
            entity.isEnemy = true;

            var view = entityConfig.GetProperty<ViewData>();
            if (view != null)
                entity.AddViewPrefab(view.Prefab);

            return entity;
        }
    }
}