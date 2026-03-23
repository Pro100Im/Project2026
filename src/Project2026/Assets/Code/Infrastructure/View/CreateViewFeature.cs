using Code.Infrastructure.Systems;
using Code.Infrastructure.View.Systems;
using Unity.Netcode;

namespace Code.Infrastructure.View
{
    public sealed class CreateViewFeature : Feature
    {
        public CreateViewFeature(ISystemFactory systems)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                Add(systems.Create<CreateEntityViewFromPathSystem>());
                Add(systems.Create<CreateEntityViewFromPrefabSystem>());
            }

            if (NetworkManager.Singleton.IsClient)
            {
                
            }

            Add(systems.Create<PlayerCharacterLinkSystem>());
        }
    }
}