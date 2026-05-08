using Code.Game.Common.Entity;
using Entitas;

namespace Code.Game.Features.Damage.Systems
{
    public class ApplyDamageSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _damages;

        public ApplyDamageSystem(GameContext gameContext)
        {
            _damages = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.TotalDamage,
                    GameMatcher.OwnerId,
                    GameMatcher.TargetId,
                    GameMatcher.DamageRequest));
        }

        public void Execute()
        {
            foreach (var damage in _damages)
            {
                var targetId = damage.targetId.Value;
                var target = GetGameEntityById.Get(targetId);
                var damageAmount = damage.totalDamage.Value;

                if (target.hasCurrentHealth)
                {
                    var newCurrentHealth = target.currentHealth.Value - damageAmount;

                    if (newCurrentHealth < 0)
                    {
                        newCurrentHealth = 0;
                    }

                    target.ReplaceCurrentHealth(newCurrentHealth);
                }

                damage.isDestructed = true;
            }
        }
    }
}