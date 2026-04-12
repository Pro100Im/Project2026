using Code.Game.Common.Destruct.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Common.Destruct
{
    public class ProcessDestructedFeature : Feature
    {
        public ProcessDestructedFeature(ISystemFactory systems)
        {
            Add(systems.Create<DelayDestructSystem>());

            Add(systems.Create<MetaDestructedSystem>());

            Add(systems.Create<GameDestructedViewSystem>());
            Add(systems.Create<GameDestructedSystem>());
        }
    }
}