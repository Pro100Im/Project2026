using Code.Game.Features.Town.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Town
{
    public class TownFeature : Feature
    {
        public TownFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<TownBuildMenuToggleSystem>());
            Add(systemFactory.Create<TownBuildMenuOpenSystem>());
            Add(systemFactory.Create<TownBuildSystem>());
            Add(systemFactory.Create<TownBuildingViewSystem>());
            Add(systemFactory.Create<TownBuildMenuRenderSystem>());
        }
    }
}
