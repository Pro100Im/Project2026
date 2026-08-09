using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Target.Systems
{
    public class RequestTargetCellSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _units;

        private readonly List<GameEntity> _buffer = new(256);

        public RequestTargetCellSystem()
        {
            _units = Contexts.sharedInstance.game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Transform,
                    GameMatcher.CurrentCell,
                    GameMatcher.Id,
                    GameMatcher.UnitSize,
                    GameMatcher.Team,
                    GameMatcher.MovementAvailable,
                    GameMatcher.GridMovement)
                .NoneOf(
                    GameMatcher.TargetCellRequest));
        }

        public void Execute()
        {
            var units = _units.GetEntities(_buffer);

            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];

                if (!unit.hasTargetCell || unit.targetCell.Value == unit.currentCell.Value)
                    unit.isTargetCellRequest = true;
            }
        }
    }
}
