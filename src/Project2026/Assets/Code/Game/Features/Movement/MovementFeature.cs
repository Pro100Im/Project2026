using Code.Game.Features.Movement.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Movement
{
    public class MovementFeature : Feature
    {
        public MovementFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<OccupiedCellSystem>());
            Add(systemFactory.Create<ReservedCellSystem>());
            Add(systemFactory.Create<SelectTargetCellSystem>());
            Add(systemFactory.Create<FlipAlongMoveDirectionSystem>());
            Add(systemFactory.Create<MovementSystem>()); 
            Add(systemFactory.Create<AttachPosToTargetSystem>()); 
        }
    }
}