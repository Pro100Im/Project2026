using Code.Common.Entity;
using Entitas;
using UnityEngine.UIElements;

namespace Code.Game.Features.Health.Systems
{
    public class HealthBarSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _hpBars;

        public HealthBarSystem(GameContext gameContext)
        {
            _hpBars = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.UIDocument,
                    GameMatcher.OwnerId,
                    GameMatcher.CurrentHealth));
        }

        public void Execute()
        {
            foreach(var hpBar in _hpBars)
            {
                var owner = GetGameEntityById.Get(hpBar.ownerId.Value);

                if (hpBar.currentHealth.Value == owner.currentHealth.Value)
                    continue;

                var healthRatio = owner.currentHealth.Value / owner.maxHealth.Value;
                var healthPercent = healthRatio * 100f;
                var uiDocument = hpBar.uIDocument.Value;
                var hpBarView = uiDocument.rootVisualElement.Q<Image>("HpFillFront");

                hpBarView.style.width = Length.Percent(healthPercent);

                hpBar.ReplaceCurrentHealth(owner.currentHealth.Value);
            }
        }
    }
}