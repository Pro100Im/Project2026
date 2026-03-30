using Code.Game.Features.Movement.Systems;
using Code.Game.Features.Player.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Movement
{
    public class MovementFeature : Feature
    {
        public MovementFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<EnemiesMovementSystem>()); 
        }
    }
}