using Code.Game.Common.Entity;
using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Unit.Systems
{
    public class UnitRangeViewSystem : ReactiveSystem<InputEntity>
    {
        private readonly IGroup<MetaEntity> _rangeView;

        public UnitRangeViewSystem(InputContext inputContext, MetaContext metaContext)
            : base(inputContext)
        {
            _rangeView = metaContext.GetGroup(MetaMatcher.UnitRangeView);
        }

        protected override ICollector<InputEntity> GetTrigger(IContext<InputEntity> context) =>
            context.CreateCollector(InputMatcher.EntityInteractIntent.Added());

        protected override bool Filter(InputEntity entity) => entity.isEntityInteractIntent;

        protected override void Execute(List<InputEntity> entities)
        {
            var rangeView = _rangeView.GetSingleEntity();

            if (rangeView == null)
                return;

            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];

                if (!entity.hasTargetId)
                {
                    Deselect(rangeView);
                    continue;
                }

                var targetEntity = GetGameEntityById.Get(entity.targetId.Value);

                if (targetEntity == null)
                {
                    Deselect(rangeView);
                    continue;
                }

                if (rangeView.hasTargetId && rangeView.targetId.Value == targetEntity.id.Value)
                    continue;

                rangeView.ReplaceTargetId(targetEntity.id.Value);

                if (targetEntity.hasRange && targetEntity.hasTransform)
                    Show(rangeView, targetEntity);
                else
                    Hide(rangeView);
            }
        }

        private static void Show(MetaEntity rangeView, GameEntity targetEntity)
        {
            var pos = targetEntity.woldPos.Value;

            if (targetEntity.hasUnitAnchorPoint)
                pos += targetEntity.unitAnchorPoint.Value;

            rangeView.unitRangeView.Value.ShowRangeView(pos, TargetService.GetEffectiveRange(targetEntity.range.Value));
            rangeView.isUnitRangeShowed = true;
            rangeView.isAbilityRangeShowed = false;
        }

        private static void Hide(MetaEntity rangeView)
        {
            if (!rangeView.isUnitRangeShowed)
                return;

            rangeView.unitRangeView.Value.HideRangeView();
            rangeView.isUnitRangeShowed = false;
        }

        private static void Deselect(MetaEntity rangeView)
        {
            Hide(rangeView);

            if (rangeView.hasTargetId)
                rangeView.RemoveTargetId();
        }
    }
}
