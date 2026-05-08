using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;

namespace Assets.Code.Game.Features.Target.Systems
{
    public class RequestTargetCellSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _units;

        private readonly List<GameEntity> _buffer = new(128);

        public RequestTargetCellSystem(GameContext context, TargetService targetService)
        {
            _units = context.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Transform,
                    GameMatcher.CurrentCell,
                    GameMatcher.Id,
                    GameMatcher.UnitSize,
                    GameMatcher.Team,
                    GameMatcher.MovementAvailable)
                .NoneOf(
                    GameMatcher.TargetCellRequest));
        }

        public void Execute()
        {
            foreach (var unit in _units.GetEntities(_buffer))
            {
                if (!unit.isTargetCellRequest)
                {
                    if (!unit.hasTargetCell || unit.targetCell.Value == unit.currentCell.Value)
                    {
                        unit.isTargetCellRequest = true;
                    }
                }
            }
        }
    }
}