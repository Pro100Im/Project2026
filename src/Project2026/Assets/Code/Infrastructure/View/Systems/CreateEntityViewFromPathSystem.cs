using Code.Infrastructure.AssetManagement;
using Code.Infrastructure.View;
using Code.Infrastructure.View.Pool;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Infrastructure.View.Systems
{
    public class CreateEntityViewFromPathSystem : IExecuteSystem
    {
        private readonly IAssetProvider _assetProvider;
        private readonly IEntityViewPool _pool;

        private readonly IGroup<GameEntity> _entities;
        private readonly List<GameEntity> _buffer = new(32);

        public CreateEntityViewFromPathSystem(GameContext game, IAssetProvider assetProvider, IEntityViewPool pool)
        {
            _assetProvider = assetProvider;
            _pool = pool;

            _entities = game.GetGroup(GameMatcher
              .AllOf(GameMatcher.ViewPath)
              .NoneOf(GameMatcher.View));
        }

        public void Execute()
        {
            var entities = _entities.GetEntities(_buffer);

            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];

                var viewPrefab = _assetProvider.LoadAsset<EntityBehaviour>(entity.viewPath.Value);
                var view = _pool.Get(viewPrefab, entity.spawnPosition.Value, Quaternion.identity);

                view.SetEntity(entity);

                entity.RemoveSpawnPosition();
                entity.RemoveViewPath();
            }
        }
    }
}
