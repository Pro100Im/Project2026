using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Health.Systems
{
    /// <summary>
    /// Pushes the owner's health into its HP bar fill when it changes; the back fill then catches up
    /// (HpBarBackFillSystem). The bar is drawn by HpBarRenderSystem.
    /// </summary>
    public class HealthBarSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _hpBars;

        private readonly List<GameEntity> _hpBarsBuffer = new(512);

        public HealthBarSystem()
        {
            _hpBars = Contexts.sharedInstance.game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.HpBar,
                    GameMatcher.HpBarFill,
                    GameMatcher.OwnerId,
                    GameMatcher.CurrentHealth));
        }

        public void Execute()
        {
            var hpBars = _hpBars.GetEntities(_hpBarsBuffer);

            for (var i = 0; i < hpBars.Count; i++)
            {
                var hpBarEntity = hpBars[i];
                var owner = GetGameEntityById.Get(hpBarEntity.ownerId.Value);

                if (owner == null || !owner.hasCurrentHealth || !owner.hasMaxHealth)
                {
                    hpBarEntity.isDestructed = true;
                    continue;
                }

                if (hpBarEntity.currentHealth.Value == owner.currentHealth.Value)
                    continue;

                var healthRatio = owner.maxHealth.Value > 0
                    ? (float)owner.currentHealth.Value / owner.maxHealth.Value
                    : 0f;

                // Mutated in place: render data, nothing reacts to it.
                var fill = hpBarEntity.hpBarFill;
                fill.Value.BackFrom = fill.Value.BackFill;
                fill.Value.Fill = healthRatio < 0f ? 0f : healthRatio;
                fill.Value.Elapsed = 0f;

                hpBarEntity.ReplaceCurrentHealth(owner.currentHealth.Value);

                if (hpBarEntity.currentHealth.Value <= 0)
                    hpBarEntity.isDestructed = true;
            }
        }
    }
}
