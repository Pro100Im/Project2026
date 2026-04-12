using Code.Infrastructure.AssetManagement;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Infrastructure.View.Systems
{
    public class CreateEntityViewFromPathSystem : IExecuteSystem
    {
        private readonly IAssetProvider _assetProvider;

        private readonly IGroup<GameEntity> _entities;
        private readonly List<GameEntity> _buffer = new(32);

        public CreateEntityViewFromPathSystem(GameContext game, IAssetProvider assetProvider)
        {
            _assetProvider = assetProvider;

            _entities = game.GetGroup(GameMatcher
              .AllOf(GameMatcher.ViewPath)
              .NoneOf(GameMatcher.View));
        }

        public void Execute()
        {
            foreach (GameEntity entity in _entities.GetEntities(_buffer))
            {
                var viewPrefab = _assetProvider.LoadAsset<EntityBehaviour>(entity.viewPath.Value);
                var view = GameObject.Instantiate<EntityBehaviour>(viewPrefab, entity.spawnPosition.Value, Quaternion.identity, null);

                view.SetEntity(entity);

                entity.RemoveSpawnPosition();
                entity.RemoveViewPath();
            }
        }
    }
}