using Code.Game.Features.LevelLoad.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.LevelLoad
{
    public class LevelLoadFeature : Feature
    {
        public LevelLoadFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<RequestSelectedLevelSystem>());
            Add(systemFactory.Create<SpawnLevelSystem>());
            Add(systemFactory.Create<LevelStarsSystem>());
            Add(systemFactory.Create<LevelCompletionSystem>());
        }
    }
}
