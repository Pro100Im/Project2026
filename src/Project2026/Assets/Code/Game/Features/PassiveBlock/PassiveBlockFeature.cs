using Code.Game.Features.PassiveBlock.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.PassiveBlock
{
    public class PassiveBlockFeature : Feature
    {
        public PassiveBlockFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<BlockEndSystem>());
            Add(systemFactory.Create<PassiveBlockSystem>());
        }
    }
}
