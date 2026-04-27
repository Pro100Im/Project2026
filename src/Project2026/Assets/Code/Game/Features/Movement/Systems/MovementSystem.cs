using Code.Game.Common.Time;
using Entitas;
using UnityEngine;

namespace Code.Game.Features.Movement.Systems
{
    public class MovementSystem : IExecuteSystem
    {
        private readonly ITimeService _timeService;
        private readonly IGroup<GameEntity> _units;
        private readonly IGroup<GameEntity> _maps;

        private const float ArriveThreshold = 0.1f;
        private const float WaitSeconds = 1f;

        public MovementSystem(GameContext context, ITimeService timeService)
        {
            _timeService = timeService;

            _units = context.GetGroup(GameMatcher.AllOf(
                GameMatcher.Transform,
                GameMatcher.MovementSpeed,
                GameMatcher.CurrentCell));

            _maps = context.GetGroup(GameMatcher.AllOf(
                GameMatcher.FlowField,
                GameMatcher.IntegrationField,
                GameMatcher.TilemapMovement));
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();
            var flow = map.flowField.Value;
            var integration = map.integrationField.Value;
            var tilemap = map.tilemapMovement.Value;
            var occupied = map.occupField.Value;
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

                var candidates = new[]
                {
                cell + dir,
                cell + new Vector3Int(-dir.y, dir.x, 0),
                cell + new Vector3Int(dir.y, -dir.x, 0),
                cell - dir};

                var currentCost = integration.TryGetValue(cell, out var cc) ? cc : int.MaxValue;
                var chosen = cell;
                var bestCost = currentCost;
                var found = false;
                var lastDir = unit.hasLastDirection ? unit.lastDirection.Value : Vector3Int.zero;

                foreach (var cand in candidates)
                {
                    if (!tilemap.ContainsKey(cand)) 
                        continue;

                    if (occupied.ContainsKey(cand)) 
                        continue;

                    var candCost = integration.TryGetValue(cand, out var cost) ? cost : int.MaxValue;
                    var candDir = cand - cell;

                    if (candDir == -lastDir) continue;

                    if (candCost < bestCost)
                    {
                        bestCost = candCost;
                        chosen = cand;
                        found = true;
                    }
                    else if (!found && candCost == bestCost)
                    {
                        chosen = cand;
                        found = true;
                    }
                }

                if (!found)
                {
                    unit.ReplaceWaitTimer(WaitSeconds);
                    unit.isMoving = false;

                    continue;
                }

                var targetPos = tilemap[chosen];
                var currentPos = unit.transform.Value.position;
                var dirVec = (targetPos - currentPos);
                var dist = dirVec.magnitude;

                if (dist > 0f) 
                    dirVec /= dist;

                var speed = unit.movementSpeed.Value * _timeService.DeltaTime;
                var newPos = currentPos + dirVec * speed;

                unit.transform.Value.position = newPos;
                unit.isMoving = true;

                if (Vector3.Distance(newPos, targetPos) < ArriveThreshold)
                {
                    unit.ReplaceCurrentCell(chosen);
                    unit.ReplaceLastDirection(chosen - cell);
                }
            }
        }
    }
}