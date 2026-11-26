using Code.Game.Features.Movement.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Movement
{
    /// <summary>Continuous crowd movement of units. Runs inside the fixed simulation step.</summary>
    public class CrowdFeature : Feature
    {
        public CrowdFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<SyncSimUnitsSystem>());
            Add(systemFactory.Create<UpdateCongestionSystem>());
            Add(systemFactory.Create<CrowdSteeringSystem>());
            Add(systemFactory.Create<CrowdSeparationSystem>());
            Add(systemFactory.Create<ApplySimUnitsSystem>());
        }
    }
}
