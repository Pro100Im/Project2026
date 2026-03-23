using Code.Game.Features.Network.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Network
{
    public class NetworkFeature : Feature
    {
        public NetworkFeature(ISystemFactory systems)
        {
            Add(systems.Create<ObjectIdReceiveSystem>());
        }
    }
}