using Assets.Code.Game.Features.Debaffs.Systems;
using Code.Game.Features.Debaffs.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Debaffs
{
    public class DebuffFeature : Feature
    {
        public DebuffFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<FreezeSystem>());
            Add(systemFactory.Create<MoveSlowingDownSystem>());
            Add(systemFactory.Create<CombustionSystem>());
        }
    }
}