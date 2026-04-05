using Code.Game.Features.Damage.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Damage
{
    public class DamageFeature : Feature
    {
        public DamageFeature(ISystemFactory systemFactory)
        {
            Add(systemFactory.Create<ApplyDamageSystem>());
        }
    }
}