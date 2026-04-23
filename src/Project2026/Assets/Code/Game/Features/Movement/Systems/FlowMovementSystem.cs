using Code.Game.Common.Time;
using Entitas;
using System.Linq;
using UnityEngine;

namespace Code.Game.Features.Movement.Systems
{
    public class FlowMovementSystem : IExecuteSystem
    {
        private readonly ITimeService _timeService;

        private readonly IGroup<GameEntity> _maps;
        private readonly IGroup<GameEntity> _units;

        public FlowMovementSystem(GameContext gameContext, ITimeService timeService)
        {
            _timeService = timeService;

            _maps = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.TilemapMovement,
                GameMatcher.FlowField));

            _units = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.MovementAvailable,
                GameMatcher.MovementSpeed,
                GameMatcher.CurrentCell,
                GameMatcher.Enemy));
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();
            var flow = map.flowField.Value;
            var tilemap = map.tilemapMovement.Value;

            foreach (var unit in _units)
            {
                var tr = unit.transform.Value;
                var pos = tr.position;

                var cell = map.tilemapMovement.Value.Keys
                    .OrderBy(c => Vector3.Distance(tilemap[c], pos))
                    .First();

                unit.ReplaceCurrentCell(cell);

                if (unit.hasMoveTarget)
                {
                    MoveToTarget(unit, tr);
                    continue;
                }

                if (!flow.TryGetValue(cell, out var dir))
                    continue;

                var nextCell = cell + dir;

                if (!tilemap.ContainsKey(nextCell))
                    continue;

                unit.AddMoveTarget(tilemap[nextCell]);
            }
        }

        private void MoveToTarget(GameEntity unit, Transform tr)
        {
            var pos = tr.position;
            var target = unit.moveTarget.Value;

            var toTarget = target - pos;

            if (toTarget.sqrMagnitude < 0.01f)
            {
                tr.position = target;
                unit.RemoveMoveTarget();
                return;
            }

            tr.position += toTarget.normalized *
                           unit.movementSpeed.Value *
                           _timeService.DeltaTime;
        }
    }
}