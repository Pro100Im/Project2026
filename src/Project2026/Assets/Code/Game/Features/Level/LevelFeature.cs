using Code.Game.Features.Level.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Level
{
    public class LevelFeature : Feature
    {
        public LevelFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<BuildFlowFieldSystem>());
            Add(systemFactory.Create<OccupiedCellSystem>());
            Add(systemFactory.Create<ReservedCellSystem>());
        }
    }
}