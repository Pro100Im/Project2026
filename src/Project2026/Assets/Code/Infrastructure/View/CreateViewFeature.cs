using Code.Infrastructure.Systems;
using Code.Infrastructure.View.Systems;

namespace Code.Infrastructure.View
{
    public sealed class CreateViewFeature : Feature
    {
        public CreateViewFeature(ISystemFactory systems)
        {
            Add(systems.Create<CreateEntityViewFromPathSystem>());
            Add(systems.Create<CreateEntityViewFromPrefabSystem>());
        }
    }
}