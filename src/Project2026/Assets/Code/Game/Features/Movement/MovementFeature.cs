using Code.Game.Features.Movement.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Movement
{
    /// <summary>Per-frame movement: unit view interpolation, attached views and projectiles.</summary>
    public class MovementFeature : Feature
    {
        public MovementFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<InterpolateUnitViewSystem>());
            Add(systemFactory.Create<AttachPosToTargetSystem>());

            Add(systemFactory.Create<UpdateProjectileAimSystem>());
            Add(systemFactory.Create<TrajectoryMovementSystem>());

            Add(systemFactory.Create<MovementSpeedBonusCleanUpSystem>());
        }
    }
}
