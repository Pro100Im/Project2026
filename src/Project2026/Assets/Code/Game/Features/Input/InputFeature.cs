using Code.Game.Features.Input.Systems;
using Code.Game.Input.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Input
{
    public class InputFeature : Feature
    {
        public InputFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<InitializeInputSystem>());
            Add(systemFactory.Create<CapturePointerSystem>());
            Add(systemFactory.Create<CaptureClickSystem>());
            Add(systemFactory.Create<ResolveClickTargetSystem>());
            Add(systemFactory.Create<RouteClickSystem>());

            Add(systemFactory.Create<CleanUpInputSystem>());
            Add(systemFactory.Create<TearDownInputDestructedSystem>());
        }
    }
}
