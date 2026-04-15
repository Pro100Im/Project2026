using Code.Game.Common.Entity;
using Entitas;

namespace Code.Game.Features.Health.Systems
{
    public class HealthBarSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _hpBars;

        public HealthBarSystem(GameContext gameContext)
        {
            _hpBars = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.HpBar,
                    GameMatcher.OwnerId,
                    GameMatcher.CurrentHealth));
        }

        public void Execute()
        {
            foreach (var hpBarEntity in _hpBars)
            {
                var owner = GetGameEntityById.Get(hpBarEntity.ownerId.Value);

                if (hpBarEntity.currentHealth.Value == owner.currentHealth.Value)
                    continue;

                var healthRatio = owner.currentHealth.Value / owner.maxHealth.Value;
                var hpBarView = hpBarEntity.hpBar.Value;

                hpBarView.SetHp(healthRatio);
                hpBarEntity.ReplaceCurrentHealth(owner.currentHealth.Value);

                if (hpBarEntity.currentHealth.Value <= 0)
                    hpBarEntity.isDestructed = true;
            }
        }
    }
}