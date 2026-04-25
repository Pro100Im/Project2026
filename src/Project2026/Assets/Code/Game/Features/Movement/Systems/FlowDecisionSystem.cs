using Entitas;
using UnityEngine;

namespace Code.Game.Features.Movement.Systems
{
    public class FlowDecisionSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _units;
        private readonly IGroup<GameEntity> _maps;

        public FlowDecisionSystem(GameContext ctx)
        {
            _units = ctx.GetGroup(GameMatcher.CurrentCell);
            _maps = ctx.GetGroup(GameMatcher.FlowField);
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();
            var flow = map.flowField.Value;

            foreach (var unit in _units)
            {
                var cell = unit.currentCell.Value;

                if (!flow.TryGetValue(cell, out var dir))
                    continue;

                if (dir == Vector3Int.zero)
                {
                    unit.isMoving = false;
                    continue;
                }

                unit.isMoving = true;
            }
        }
    }
}