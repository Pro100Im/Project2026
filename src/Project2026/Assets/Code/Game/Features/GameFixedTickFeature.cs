using Code.Game.Features.Movement;
using Code.Game.Features.Spawn;
using Code.Infrastructure.Systems;
using Code.Infrastructure.View;

namespace Code.Game.Features
{
    public class GameFixedTickFeature : Feature
    {
        public GameFixedTickFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<CreateViewFeature>());
            Add(systemFactory.Create<SpawnFeature>());
            Add(systemFactory.Create<MovementFeature>());
        }
    }
}
