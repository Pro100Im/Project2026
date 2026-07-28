using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Ability.Systems
{
    public class AbilityCastRequestSystem : IExecuteSystem
    {
        private readonly IGroup<InputEntity> _castIntents;
        private readonly IGroup<GameEntity> _targeting;

        private readonly List<InputEntity> _castBuffer = new(4);
        private readonly List<GameEntity> _targetingBuffer = new(2);

        public AbilityCastRequestSystem(InputContext inputContext, GameContext gameContext)
        {
            _castIntents = inputContext.GetGroup(InputMatcher
                .AllOf(
                    InputMatcher.AbilityCastIntent,
                    InputMatcher.WorldPointerInput)
                .NoneOf(InputMatcher.Destructed));

            _targeting = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.AbilityTargeting,
                    GameMatcher.AbilityRadius,
                    GameMatcher.AbilityTargetFilter,
                    GameMatcher.FreezeEffect,
                    GameMatcher.Team,
                    GameMatcher.Id)
                .NoneOf(GameMatcher.Destructed));
        }

        public void Execute()
        {
            var casts = _castIntents.GetEntities(_castBuffer);

            if (casts.Count == 0)
                return;

            var targeting = _targeting.GetEntities(_targetingBuffer);
            var targetPoint = casts[0].worldPointerInput.Value;

            for (var i = 0; i < targeting.Count; i++)
            {
                var entity = targeting[i];

                entity.isAbilityTargeting = false;
                entity.isAbilityRangeShowed = false;
                entity.AddTargetPoint(targetPoint);
                entity.isAbilityCastRequest = true;
            }
        }
    }
}
