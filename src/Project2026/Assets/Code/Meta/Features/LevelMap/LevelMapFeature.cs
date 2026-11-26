using Code.Infrastructure.Systems;
using Code.Meta.Features.LevelMap.Systems;

namespace Code.Meta.Features.LevelMap
{
    public class LevelMapFeature : Feature
    {
        public LevelMapFeature(ISystemFactory systems)
        {
            Add(systems.Create<LevelMapMenuSpawnSystem>());
            Add(systems.Create<LevelNodeSpawnSystem>());
            Add(systems.Create<LevelMapMenuToggleSystem>());
            Add(systems.Create<LevelNodeProgressSystem>());
            Add(systems.Create<LevelNodeStateSystem>());
            Add(systems.Create<LevelMapRenderSystem>());
            Add(systems.Create<LevelNodeSeenSystem>());
            Add(systems.Create<LevelNodeChooseSystem>());
        }
    }
}
