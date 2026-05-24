using Code.Game.Common.Entity;
using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Unit.Systems
{
    public class UnitRangeViewSystem : ReactiveSystem<InputEntity>
    {
        private RangeViewService _rangeViewService;

        public UnitRangeViewSystem(InputContext inputContext, RangeViewService rangeViewService)
            : base(inputContext)
        {
            _rangeViewService = rangeViewService;
        }

        protected override ICollector<InputEntity> GetTrigger(IContext<InputEntity> context) =>
            context.CreateCollector(InputMatcher
                .AllOf(
                InputMatcher.Input
                ));

        protected override bool Filter(InputEntity entity) => entity.isInput;

        protected override void Execute(List<InputEntity> entities)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];

                if (entity.hasTargetId)
                {
                    var targetEntity = GetGameEntityById.Get(entity.targetId.Value);

                    if (targetEntity.hasRange && targetEntity.hasTransform)
                    {
                        var pos = targetEntity.transform.Value.position;

                        if(targetEntity.hasUnitAnchorPoint)
                            pos += targetEntity.unitAnchorPoint.Value;

                        _rangeViewService.ShowRangeView(pos, targetEntity.range.Value);
                    }

                    entity.isDestructed = true;
                }
                else
                {
                    _rangeViewService.HideRangeView();
                }

                entity.isDestructed = true;
            }
        }
    }
}