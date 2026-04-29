using Entitas;

namespace Code.Game.Features.Movement.Systems
{
    public class ReservedCellSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _units;
        private readonly IGroup<GameEntity> _maps;

        public ReservedCellSystem(GameContext context)
        {
            _units = context.GetGroup(GameMatcher.AllOf(
                GameMatcher.TargetCell,
                GameMatcher.Id));

            _maps = context.GetGroup(GameMatcher.AllOf(
                GameMatcher.ReservedField));
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();
            map.reservedField.Value.Clear();

            foreach (var unit in _units)
                map.reservedField.Value[unit.targetCell.Value] = unit.id.Value;
        }
    }
}