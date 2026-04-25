using Code.Game.Common.Time;
using Entitas;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Game.Features.Movement.Systems
{
    public class MovementSystem : IExecuteSystem
    {
        private readonly ITimeService _timeService;

        private readonly IGroup<GameEntity> _units;
        private readonly IGroup<GameEntity> _maps;

        public MovementSystem(GameContext context, ITimeService timeService)
        {
            _timeService = timeService;

            _units = context.GetGroup(GameMatcher
                .AllOf(GameMatcher.Transform, GameMatcher.MovementSpeed, GameMatcher.CurrentCell));
            _maps = context.GetGroup(GameMatcher
                .AllOf(GameMatcher.FlowField, GameMatcher.TilemapMovement, GameMatcher.TargetFlow));
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();
            var flow = map.flowField.Value;
            var tilemap = map.tilemapMovement.Value;
            var units = _units.GetEntities();

            foreach (var unit in units)
            {
                if (unit.hasWaitTimer)
                {
                    unit.ReplaceWaitTimer(unit.waitTimer.Value - _timeService.DeltaTime);
                    if (unit.waitTimer.Value <= 0)
                        unit.RemoveWaitTimer();
                    continue;
                }

                var cell = unit.currentCell.Value;

                if (!flow.TryGetValue(cell, out var dir) || dir == Vector3Int.zero)
                {
                    unit.isMoving = false;

                    continue;
                }

                var chosenCell = ChooseNextCell(cell, dir, tilemap, units);

                if (chosenCell == cell)
                {
                    unit.ReplaceWaitTimer(1f);
                    unit.isMoving = false;

                    continue;
                }

                var targetPos = tilemap[chosenCell];
                var currentPos = unit.transform.Value.position;

                var dirVec = (targetPos - currentPos).normalized;
                var speed = unit.movementSpeed.Value * _timeService.DeltaTime;

                var newPos = currentPos + dirVec * speed;

                unit.transform.Value.position = newPos;
                unit.isMoving = true;

                if (Vector3.Distance(newPos, targetPos) < 0.1f)
                    unit.ReplaceCurrentCell(chosenCell);
            }
        }

        private Vector3Int ChooseNextCell(Vector3Int cell, Vector3Int dir, Dictionary<Vector3Int, Vector3> tilemap, GameEntity[] units)
        {
            var candidates = GetMoveCandidates(cell, dir);

            foreach (var candidate in candidates)
            {
                if (!tilemap.ContainsKey(candidate))
                    continue;

                var occupied = units.Any(u => u.currentCell.Value == candidate);

                if (occupied) 
                    continue;

                return candidate;
            }

            return cell;
        }

        private List<Vector3Int> GetMoveCandidates(Vector3Int cell, Vector3Int dir)
        {
            var list = new List<Vector3Int>();
            var left = new Vector3Int(-dir.y, dir.x, 0);
            var right = new Vector3Int(dir.y, -dir.x, 0);

            list.Add(cell + dir);
            list.Add(cell + left);
            list.Add(cell + right);
            list.Add(cell - dir);

            return list;
        }
    }
}