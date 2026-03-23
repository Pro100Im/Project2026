using Code.Common.Network;
using Code.Game.Features.Network;
using Code.Infrastructure.AssetManagement;
using Unity.Netcode;
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
            var networkObject = view.GetComponent<NetworkObject>();

            networkObject.SpawnWithOwnership(entity.clientId.Value);

            var totalSize = sizeof(ulong) + sizeof(ulong);
            using var builder = new NetworkMessageBuilder(totalSize);
            var writer = builder.Write(networkObject.OwnerClientId).Write(networkObject.NetworkObjectId).Build(); 

            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                       RequestTypes.ReceiveObjectId.ToString(),
                       NetworkManager.Singleton.ConnectedClientsIds,
                       writer);

            return view;
        }

        public EntityBehaviour CreateViewForEntityFromPrefab(GameEntity entity)
        {
            var view = GameObject.Instantiate<EntityBehaviour>(entity.viewPrefab.Value, _farAway, Quaternion.identity, null);
            view.GetComponent<NetworkObject>().SpawnWithOwnership(entity.clientId.Value);

            return view;
        }
    }
} 