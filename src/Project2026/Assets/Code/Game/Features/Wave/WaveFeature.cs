using Code.Game.Features.Wave.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Wave
{
    public class WaveFeature : Feature
    {
        public WaveFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<WaveInitSystem>());
            Add(systemFactory.Create<WaveStartSystem>());
            Add(systemFactory.Create<WaveProgressSystem>());
        }
    }
}