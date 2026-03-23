using Code.Game.Features.Input;
using Code.Game.Features.Network;
using Code.Game.Features.Player;
using Code.Infrastructure.Systems;

namespace Code.Game.Features
{
    public class GameTickFeature : Feature
    {
        public GameTickFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<NetworkFeature>());
            Add(systemFactory.Create<InputFeature>());
            Add(systemFactory.Create<PlayerFeature>());
        }
    }
}
