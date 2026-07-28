using Code.Game.Features.Tower.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Tower
{
    public class TowerFeature : Feature
    {
        public TowerFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<TowerMenuSystem>());
            Add(systemFactory.Create<TowerMenuCloseSystem>());
            Add(systemFactory.Create<TowerMenuAutoCloseSystem>());
            Add(systemFactory.Create<TowerBuildSystem>());
            Add(systemFactory.Create<TowerUpgradeSystem>());
        }
    }
}