using Code.Game.Features.GameSession;
using Code.Game.Features.Input;
using Code.Game.Features.Pause;
using Code.Game.Features.Player;
using Code.Infrastructure.Systems;

namespace Code.Game.Features
{
    public class GameTickFeature : Feature
    {
        public GameTickFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<GameSessionFeature>());
            Add(systemFactory.Create<InputFeature>());
            Add(systemFactory.Create<PauseFeature>());
            Add(systemFactory.Create<PlayerFeature>());
            Add(systemFactory.Create<GameplayFeature>());
        }
    }
}
