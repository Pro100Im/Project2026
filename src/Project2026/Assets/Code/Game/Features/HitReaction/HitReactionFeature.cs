using Code.Game.Features.HitReaction.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.HitReaction
{
    public class HitReactionFeature : Feature
    {
        public HitReactionFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<PlayHitReactionSystem>());
        }
    }
}
