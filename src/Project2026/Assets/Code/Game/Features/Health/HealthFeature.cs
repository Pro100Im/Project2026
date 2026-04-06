using Code.Game.Features.Health.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Health
{
    public class HealthFeature : Feature
    {
        public HealthFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<HealthBarSystem>());
        }
    }
}