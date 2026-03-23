using Code.Infrastructure.Systems;
using Unity.Netcode;

namespace Code.Game.Features.Movement
{
    public class MovementFeature : Feature
    {
        public MovementFeature(ISystemFactory systemFactory)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {

            }

            if (NetworkManager.Singleton.IsClient)
            {

            }
        }
    }
}