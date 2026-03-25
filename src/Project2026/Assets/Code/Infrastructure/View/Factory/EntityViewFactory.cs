using Code.Infrastructure.AssetManagement;
using UnityEngine;

namespace Code.Infrastructure.View.Factory
{
    public class EntityViewFactory : IEntityViewFactory
    {
        private readonly IAssetProvider _assetProvider;
        private readonly Vector3 _farAway = new(-999, 999, 999);

        public EntityViewFactory(IAssetProvider assetProvider)
        {
            _assetProvider = assetProvider;
        }

        public EntityBehaviour CreateViewForEntity(GameEntity entity)
        {
            var viewPrefab = _assetProvider.LoadAsset<EntityBehaviour>(entity.viewPath.Value);
            var view = GameObject.Instantiate<EntityBehaviour>(viewPrefab, _farAway, Quaternion.identity, null);

            return view;
        }

        public EntityBehaviour CreateViewForEntityFromPrefab(GameEntity entity)
        {
            var view = GameObject.Instantiate<EntityBehaviour>(entity.viewPrefab.Value, _farAway, Quaternion.identity, null);

            return view;
        }
    }
} 