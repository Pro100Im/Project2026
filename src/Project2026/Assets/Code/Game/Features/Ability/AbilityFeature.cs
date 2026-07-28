using Code.Game.Features.Ability.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Ability
{
    public class AbilityFeature : Feature
    {
        public AbilityFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<AbilityCancelSystem>());
            Add(systemFactory.Create<AbilitySelectSystem>());
            Add(systemFactory.Create<AbilityCastRequestSystem>());
            Add(systemFactory.Create<AbilityTargetingPreviewSystem>());
            Add(systemFactory.Create<AbilityCastSystem>());
        }
    }
}
