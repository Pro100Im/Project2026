using Code.Game.Features.Cooldown.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Cooldown
{
    public class CooldownFeature : Feature
    {
        public CooldownFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<CooldownLeftSystem>());
        }
    }
}