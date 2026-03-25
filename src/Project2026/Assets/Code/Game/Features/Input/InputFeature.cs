using Code.Game.Features.Input.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Input
{
    public class InputFeature : Feature
    {
        public InputFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<InitializeInputSystem>());
            Add(systemFactory.Create<EmitInputSystem>());
        }
    }
}