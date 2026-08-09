using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Unit.Systems
{
    public class UnitRangeViewRefreshSystem : ReactiveSystem<GameEntity>
    {
        private readonly IGroup<MetaEntity> _rangeView;

        public UnitRangeViewRefreshSystem() : base(Contexts.sharedInstance.game)
        {
            _rangeView = Contexts.sharedInstance.meta.GetGroup(MetaMatcher.UnitRangeView);
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context) =>
            context.CreateCollector(GameMatcher.Range.Added());

        protected override bool Filter(GameEntity entity) =>
            entity.hasId && entity.hasRange && entity.hasTransform;

        protected override void Execute(List<GameEntity> entities)
        {
            var rangeView = _rangeView.GetSingleEntity();

            if (rangeView == null || !rangeView.hasTargetId || rangeView.isAbilityRangeShowed)
                return;

            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];

                if (entity.id.Value != rangeView.targetId.Value)
                    continue;

                var pos = entity.woldPos.Value;

                if (entity.hasUnitAnchorPoint)
                    pos += entity.unitAnchorPoint.Value;

                rangeView.unitRangeView.Value.ShowRangeView(pos, TargetService.GetEffectiveRange(entity.range.Value));
                rangeView.isUnitRangeShowed = true;

                return;
            }
        }
    }
}
