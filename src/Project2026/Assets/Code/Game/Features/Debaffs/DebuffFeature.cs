using Code.Game.Features.Debaffs.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Debaffs
{
    public class DebuffFeature : Feature
    {
        public DebuffFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<MoveSlowingDownSystem>());
        }
    }
}