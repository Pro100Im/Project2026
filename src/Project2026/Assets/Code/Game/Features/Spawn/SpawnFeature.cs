using Code.Game.Features.Spawn.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Spawn
{
    public class SpawnFeature : Feature
    {
        public SpawnFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<EnemySelectSpawnPosSystem>());
            Add(systemFactory.Create<EnemySpawnSystem>());
        }
    }
}