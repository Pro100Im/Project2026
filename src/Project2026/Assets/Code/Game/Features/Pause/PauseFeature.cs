using Code.Game.Features.Pause.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Pause
{
    public class PauseFeature : Feature
    {
        public PauseFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<PauseSystem>());
        }
    }
}