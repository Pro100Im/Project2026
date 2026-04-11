using Code.Game.Features.Tower.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Tower
{
    public class TowerFeature : Feature
    {
        public TowerFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<TowerBuildSystem>());
        }
    }
}