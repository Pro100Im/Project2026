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
            _units = ctx.GetGroup(GameMatcher
                .AllOf(GameMatcher.CurrentCell)
                .NoneOf(GameMatcher.NextCell));

            _maps = ctx.GetGroup(GameMatcher.FlowField);
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();
            var flow = map.flowField.Value;
            var tilemap = map.tilemapMovement.Value;

            foreach (var unit in _units.GetEntities())
            {
                var cell = unit.currentCell.Value;

                if (!flow.TryGetValue(cell, out var dir))
                    continue;

                var next = cell + dir;

                if (!tilemap.ContainsKey(next))
                    continue;

                unit.AddNextCell(next);
            }
        }
    }
}