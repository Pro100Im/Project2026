using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Input.Systems
{
    public class RouteClickSystem : IExecuteSystem
    {
        private readonly IGroup<InputEntity> _clicks;
        private readonly IGroup<GameEntity> _abilityTargeting;

        private readonly List<InputEntity> _clicksBuffer = new(4);

        public RouteClickSystem(InputContext inputContext, GameContext gameContext)
        {
            _clicks = inputContext.GetGroup(InputMatcher
                .AllOf(InputMatcher.ClickInput)
                .NoneOf(InputMatcher.Destructed));

            _abilityTargeting = gameContext.GetGroup(GameMatcher
                .AllOf(GameMatcher.AbilityTargeting)
                .NoneOf(GameMatcher.Destructed));
        }

        public void Execute()
        {
            var targetingActive = _abilityTargeting.count > 0;
            var clicks = _clicks.GetEntities(_clicksBuffer);

            for (var i = 0; i < clicks.Count; i++)
            {
                var click = clicks[i];

                if (click.isCancelClick && targetingActive)
                {
                    click.isCancelIntent = true;
                    continue;
                }

                if (!click.isPrimaryClick)
                    continue;

                if (click.isPointerOverUI)
                    continue;

                if (click.isInteractTarget)
                {
                    click.isEntityInteractIntent = true;

                    if (targetingActive)
                        click.isCancelIntent = true;

                    continue;
                }

                if (targetingActive)
                {
                    click.isAbilityCastIntent = true;
                    continue;
                }

                click.isEntityInteractIntent = true;
            }
        }
    }
}
