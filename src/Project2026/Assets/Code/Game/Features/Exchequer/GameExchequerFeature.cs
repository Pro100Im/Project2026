using Code.Game.Features.Exchequer.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Exchequer
{
    public class GameExchequerFeature : Feature
    {
        public GameExchequerFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<GameGoldExchequerSystem>());
        }
    }
}