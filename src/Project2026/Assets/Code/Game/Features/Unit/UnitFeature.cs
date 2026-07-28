using Code.Game.Features.Unit.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Unit
{
    public class UnitFeature : Feature
    {
        public UnitFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<UnitRangeViewSystem>());
            Add(systemFactory.Create<UnitRangeViewRefreshSystem>());
        }
    }
}