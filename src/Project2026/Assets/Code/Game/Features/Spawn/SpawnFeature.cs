using Code.Game.Features.Player.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Spawn
{
    public class SpawnFeature : Feature
    {
        public SpawnFeature(ISystemFactory systemFactory)
        {
            //Add(systemFactory.Create<SelectPlayerSpawnPositionSystem>());
        }
    }
}