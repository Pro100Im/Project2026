using Code.Infrastructure.AssetManagement;
using UnityEngine;

namespace Code.Infrastructure.View.Factory
{
    public class EntityViewFactory : IEntityViewFactory
    {
        private readonly IAssetProvider _assetProvider;

        public EntityViewFactory(IAssetProvider assetProvider)
        {
            _assetProvider = assetProvider;
        }

        public EntityBehaviour CreateViewForEntity(GameEntity entity)
        {
            var viewPrefab = _assetProvider.LoadAsset<EntityBehaviour>(entity.viewPath.Value);
            var view = GameObject.Instantiate<EntityBehaviour>(viewPrefab, entity.spawnPosition.Value, Quaternion.identity, null);

            view.SetEntity(entity);

            return view;
        }

        public EntityBehaviour CreateViewForEntityFromPrefab(GameEntity entity)
        {
            var view = GameObject.Instantiate<EntityBehaviour>(entity.viewPrefab.Value, entity.spawnPosition.Value, Quaternion.identity, null);

            view.SetEntity(entity);

            return view;
        }
    }
} 