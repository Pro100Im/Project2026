using Code.Game.Features.Animator.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Animator
{
    public class AnimatorFeature : Feature
    {
        public AnimatorFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<CharacterAnimatorSystem>());
            Add(systemFactory.Create<PlayerCastleAnimatorSystem>());
        }
    }
}