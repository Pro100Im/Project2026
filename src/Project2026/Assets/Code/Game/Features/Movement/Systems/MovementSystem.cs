using Code.Game.Common.Time;
using Entitas;
using System.Collections.Generic;
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
                .AllOf(
                    GameMatcher.Transform,
                    GameMatcher.MovementSpeed,
                    GameMatcher.CurrentCell));

            _maps = context.GetGroup(GameMatcher
                .AllOf(GameMatcher.FlowField, GameMatcher.TilemapMovement));
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();
            var flow = map.flowField.Value;
            var tilemap = map.tilemapMovement.Value;
            var units = _units.GetEntities();
            var occupied = new HashSet<Vector3Int>();

            foreach (var u in units)
                occupied.Add(u.currentCell.Value);

            foreach (var unit in units)
            {
                var cell = unit.currentCell.Value;

                if (!flow.TryGetValue(cell, out var dir) || dir == Vector3Int.zero)
                {
                    unit.isMoving = false;
                    continue;
                }

                var candidates = GetMoveCandidates(cell, dir);
                var chosenCell = cell;
                var found = false;

                foreach (var candidate in candidates)
                {
                    if (!tilemap.ContainsKey(candidate))
                        continue;

                    if (occupied.Contains(candidate))
                        continue;

                    chosenCell = candidate;
                    found = true;

                    break;
                }

                if (!found)
                {
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