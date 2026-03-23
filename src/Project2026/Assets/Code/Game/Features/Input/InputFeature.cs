using Code.Game.Features.Input.Systems;
using Code.Infrastructure.Systems;
using Unity.Netcode;

namespace Code.Game.Features.Input
{
    public class InputFeature : Feature
    {
        public InputFeature(ISystemFactory systemFactory)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                
            }

            if (NetworkManager.Singleton.IsClient)
            {
                Add(systemFactory.Create<InitializeInputSystem>());
                Add(systemFactory.Create<EmitInputSystem>());
            }
        }
    }
}