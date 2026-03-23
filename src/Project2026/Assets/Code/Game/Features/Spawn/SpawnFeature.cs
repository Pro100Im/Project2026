using Code.Game.Features.Player.Systems;
using Code.Infrastructure.Systems;
using Unity.Netcode;

namespace Code.Game.Features.Spawn
{
    public class SpawnFeature : Feature
    {
        public SpawnFeature(ISystemFactory systemFactory)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                Add(systemFactory.Create<SelectPlayerSpawnPositionSystem>());
            }

            if (NetworkManager.Singleton.IsClient)
            {
                
            }

            Add(systemFactory.Create<SetPlayerSpawnedPositionSystem>());
        }
    }
}