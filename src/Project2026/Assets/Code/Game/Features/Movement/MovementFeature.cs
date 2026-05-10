using Code.Game.Features.Movement.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Movement
{
    public class MovementFeature : Feature
    {
        public MovementFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<FlipAlongMoveDirectionSystem>());
            Add(systemFactory.Create<GridMovementSystem>()); 
            Add(systemFactory.Create<AttachPosToTargetSystem>());
            Add(systemFactory.Create<TrajectoryMovementSystem>());
        }
    }
}