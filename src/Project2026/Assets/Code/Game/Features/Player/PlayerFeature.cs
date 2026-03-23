using Code.Game.Features.Player.Systems;
using Code.Infrastructure.Systems;
using Unity.Netcode;

namespace Code.Game.Features.Player
{
    public class PlayerFeature : Feature
    {
        public PlayerFeature(ISystemFactory systemFactory)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                
            }

            if(NetworkManager.Singleton.IsClient)
            {
                Add(systemFactory.Create<PlayerCameraInitSystem>());
                Add(systemFactory.Create<PlayerAnimatorSystem>());
            }
        }
    }
}