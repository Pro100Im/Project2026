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

                if (entity.hasTargetId)
                {
                    var targetEntity = GetGameEntityById.Get(entity.targetId.Value);

                    if (rangeView.isUnitRangeShowed && (rangeView.hasTargetId && rangeView.targetId.Value == targetEntity.id.Value || !targetEntity.hasRange))
                    {
                        rangeView.unitRangeView.Value.HideRangeView();
                        rangeView.isUnitRangeShowed = false;
                    }
                    else if (targetEntity.hasRange && targetEntity.hasTransform)
                    {
                        var pos = targetEntity.woldPos.Value;

                        if (targetEntity.hasUnitAnchorPoint)
                            pos += targetEntity.unitAnchorPoint.Value;

                        rangeView.unitRangeView.Value.ShowRangeView(pos, TargetService.GetEffectiveRange(targetEntity.range.Value));
                        rangeView.ReplaceTargetId(targetEntity.id.Value);
                        rangeView.isUnitRangeShowed = true;
                        rangeView.isAbilityRangeShowed = false;
                    }
                }
                else if (rangeView.isUnitRangeShowed)
                {
                    rangeView.unitRangeView.Value.HideRangeView();
                    rangeView.isUnitRangeShowed = false;
                }
            }
        }
    }
}
