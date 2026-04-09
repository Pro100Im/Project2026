using Code.Game.Features.Death.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Death
{
    public class DeathFeature : Feature
    {
        public DeathFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<DeathSystem>());
        }
    }
}