using Code.Game.Features.Town.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Town
{
    public class TownFeature : Feature
    {
        public TownFeature(ISystemFactory systemFactory)
        {
            //Add(systemFactory.Create<TownMenuSystem>());
            Add(systemFactory.Create<TownTestSystem>());
        }
    }
}