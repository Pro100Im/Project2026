using Code.Game.Features.Player.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Player
{
    public class PlayerFeature : Feature
    {
        public PlayerFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<PlayerCameraInitSystem>());
            Add(systemFactory.Create<PlayerAnimatorSystem>());
        }
    }
}