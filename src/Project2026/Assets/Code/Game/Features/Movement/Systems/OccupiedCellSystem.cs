using Entitas;

namespace Code.Game.Features.Movement.Systems
{
    public class OccupiedCellSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _units;
        private readonly IGroup<GameEntity> _maps;

        public OccupiedCellSystem(GameContext context)
        {
            _units = context.GetGroup(GameMatcher.AllOf(
                GameMatcher.Transform,
                GameMatcher.MovementSpeed,
                GameMatcher.CurrentCell,
                GameMatcher.Id));

            _maps = context.GetGroup(GameMatcher.AllOf(
                GameMatcher.TilemapMovement,
                GameMatcher.OccupField));
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();
            map.occupField.Value.Clear();

            foreach (var unit in _units)
                map.occupField.Value.Add(unit.currentCell.Value, unit.id.Value);
        }
    }
}