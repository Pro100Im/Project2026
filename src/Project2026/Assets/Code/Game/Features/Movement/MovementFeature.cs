using Code.Game.Features.Movement.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Movement
{
    public class MovementFeature : Feature
    {
        public MovementFeature(ISystemFactory systemFactory)
        {
            // 1. Grid step (sets Moving / Velocity)
            Add(systemFactory.Create<GridMovementSystem>());
            // 2. Presentation from movement
            Add(systemFactory.Create<FlipAlongMoveDirectionSystem>());
            Add(systemFactory.Create<AttachPosToTargetSystem>());
            // 3. Projectiles
            Add(systemFactory.Create<TrajectoryMovementSystem>());
            // 4. Cleanup
            Add(systemFactory.Create<MovementSpeedBonusCleanUpSystem>());
        }
    }
}
