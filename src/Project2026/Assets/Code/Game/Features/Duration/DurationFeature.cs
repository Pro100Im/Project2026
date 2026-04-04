using Code.Game.Features.Duration.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Duration
{
    public class DurationFeature : Feature
    {
        public DurationFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<DurationLeftSystem>());
        }
    }
}