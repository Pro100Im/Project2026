using Assets.Code.Game.Features.Target.Systems;
using Code.Game.Features.Target.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Target
{
    public class TargetFeature : Feature
    {
        public TargetFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<RequestTargetCellSystem>());
            Add(systemFactory.Create<SelectTargetCellSystem>());
            Add(systemFactory.Create<CheckTargetSystem>());
        }
    }
}