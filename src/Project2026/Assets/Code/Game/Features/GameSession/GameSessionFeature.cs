using Code.Game.Features.GameSession.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.GameSession
{
    public class GameSessionFeature : Feature
    {
        public GameSessionFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<GameSessionSystem>());
            Add(systemFactory.Create<GameSessionEndMenuSystem>());
        }
    }
}