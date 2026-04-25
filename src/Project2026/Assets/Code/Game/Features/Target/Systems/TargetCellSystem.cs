using Code.Game.Common.Entity;
using Entitas;

namespace Code.Game.Features.Target.Systems
{
    public class TargetCellSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _units;

        public TargetCellSystem(GameContext ctx)
        {
            _units = ctx.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Targetable,
                    GameMatcher.Player,
                    GameMatcher.Id));
        }

        public void Execute()
        {
            foreach (var unit in _units)
            {
                var target = GetGameEntityById.Get(unit.id.Value);

                if (target == null || !target.hasCurrentCell)
                    continue;

                //unit.Replace(target.currentCell.Value);
            }
        }
    }
}