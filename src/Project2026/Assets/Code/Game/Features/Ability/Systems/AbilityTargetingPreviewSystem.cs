using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Ability.Systems
{
    public class AbilityTargetingPreviewSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _targeting;
        private readonly IGroup<MetaEntity> _rangeViews;
        private readonly IGroup<InputEntity> _pointers;

        private readonly List<GameEntity> _targetingBuffer = new(2);
        private readonly List<InputEntity> _pointersBuffer = new(1);

        public AbilityTargetingPreviewSystem(
            GameContext gameContext,
            MetaContext metaContext,
            InputContext inputContext)
        {
            _targeting = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.AbilityTargeting,
                    GameMatcher.AbilityRadius)
                .NoneOf(GameMatcher.Destructed));

            _rangeViews = metaContext.GetGroup(MetaMatcher.UnitRangeView);

            _pointers = inputContext.GetGroup(InputMatcher
                .AllOf(
                    InputMatcher.PointerState,
                    InputMatcher.WorldPointerInput)
                .NoneOf(InputMatcher.Destructed));
        }

        public void Execute()
        {
            var rangeView = _rangeViews.GetSingleEntity();

            if (rangeView == null)
                return;

            var targeting = _targeting.GetEntities(_targetingBuffer);

            if (targeting.Count == 0)
            {
                HideAbilityRange(rangeView);

                return;
            }

            var pointers = _pointers.GetEntities(_pointersBuffer);

            if (pointers.Count == 0)
                return;

            var ability = targeting[0];
            var position = (Vector3)pointers[0].worldPointerInput.Value;

            if (!ability.isAbilityRangeShowed)
            {
                if (rangeView.isUnitRangeShowed)
                    rangeView.isUnitRangeShowed = false;

                rangeView.unitRangeView.Value.ShowRangeView(position, ability.abilityRadius.Value);
                ability.isAbilityRangeShowed = true;

                return;
            }

            rangeView.unitRangeView.Value.UpdateRangeView(position, ability.abilityRadius.Value);
        }

        private void HideAbilityRange(MetaEntity rangeView)
        {
            rangeView.unitRangeView.Value.HideRangeView();
        }
    }
}
