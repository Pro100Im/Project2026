using Code.Game.Common.Time;
using Code.Game.Features.Target.Services;
using Entitas;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Game.Features.Movement.Systems
{
    public class MovementSystem : IExecuteSystem
    {
        private readonly ITimeService _timeService;
        private readonly TargetService _targetService;
        private readonly IGroup<GameEntity> _units;
        private readonly IGroup<GameEntity> _maps;

        private const float ArriveThreshold = 0.1f;
        private const float WaitSeconds = 0.5f;
        private const int MinIntegrationImprovement = 1;

        public MovementSystem(GameContext context, ITimeService timeService, TargetService targetService)
        {
            _timeService = timeService;
            _targetService = targetService;

            _units = context.GetGroup(GameMatcher.AllOf(GameMatcher.Transform, GameMatcher.MovementSpeed, GameMatcher.CurrentCell));
            _maps = context.GetGroup(GameMatcher.AllOf(GameMatcher.FlowField, GameMatcher.IntegrationField, GameMatcher.TilemapMovement, GameMatcher.TargetFlow));
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();
            var flow = map.flowField.Value;
            var integration = map.integrationField.Value;
            var tilemap = map.tilemapMovement.Value;
            var goals = map.targetFlow.Value;
            var units = _units.GetEntities();


            var occupied = new HashSet<Vector3Int>(units.Length);
            foreach (var u in units) occupied.Add(u.currentCell.Value);


            var reservedTargets = new HashSet<Vector3Int>();
            var reservedSteps = new HashSet<Vector3Int>();
            var intendedSteps = new Dictionary<GameEntity, Vector3Int>(units.Length);


            var ordered = units.OrderBy(u =>
            {
                var c = u.currentCell.Value;
                int best = int.MaxValue;
                foreach (var g in goals)
                {
                    var manh = Math.Abs(g.x - c.x) + Math.Abs(g.y - c.y);
                    best = Math.Min(best, manh);
                }
                return best;
            }).ToArray();

            foreach (var unit in ordered)
            {
                if (unit.hasWaitTimer)
                {
                    unit.ReplaceWaitTimer(unit.waitTimer.Value - _timeService.DeltaTime);
                    if (unit.waitTimer.Value <= 0) unit.RemoveWaitTimer();
                    continue;
                }

                var cell = unit.currentCell.Value;
                if (unit.hasTargetCell) continue;

                var bestTarget = FindBestTargetAroundGoals_NoAlloc(cell, integration, goals, tilemap, occupied, reservedTargets);
                if (bestTarget != cell)
                {
                    unit.ReplaceTargetCell(bestTarget);

                    reservedTargets.Add(bestTarget);
                }
                else
                {
                    unit.ReplaceWaitTimer(WaitSeconds);
                    unit.isMoving = false;
                }
            }


            foreach (var unit in ordered)
            {
                if (unit.hasWaitTimer)
                {
                    unit.ReplaceWaitTimer(unit.waitTimer.Value - _timeService.DeltaTime);
                    if (unit.waitTimer.Value <= 0) unit.RemoveWaitTimer();
                    continue;
                }

                var cell = unit.currentCell.Value;

                if (!flow.TryGetValue(cell, out var dir) || dir == Vector3Int.zero)
                {
                    unit.isMoving = false;
                    continue;
                }

                if (!unit.hasTargetCell)
                {
                    unit.isMoving = false;
                    continue;
                }

                var targetCell = unit.targetCell.Value;
                if (cell == targetCell)
                {
                    unit.isMoving = false;
                    unit.RemoveTargetCell();
                    continue;
                }

                var currentCost = integration.TryGetValue(cell, out var cc) ? cc : int.MaxValue;


                var forward = cell + dir;
                var left = cell + new Vector3Int(-dir.y, dir.x, 0);
                var right = cell + new Vector3Int(dir.y, -dir.x, 0);
                var back = cell - dir;


                var candArray = new[] { forward, left, right, back };

                Vector3Int chosen = cell;
                bool found = false;


                for (int i = 0; i < candArray.Length; i++)
                {
                    var cand = candArray[i];
                    if (!tilemap.ContainsKey(cand)) continue;
                    if (occupied.Contains(cand)) continue;
                    if (reservedSteps.Contains(cand)) continue;
                    if (reservedTargets.Contains(cand) && cand != targetCell) continue;
                    if (!integration.TryGetValue(cand, out var candCost)) continue;

                    if (currentCost - candCost >= MinIntegrationImprovement)
                    {
                        bool otherPlansToOurCell = intendedSteps.Values.Any(s => s == cell);
                        if (otherPlansToOurCell) continue;

                        chosen = cand;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    float bestDist = float.MaxValue;
                    Vector3Int bestEqual = cell;
                    bool foundEqual = false;

                    var curD = Vector3.Distance(new Vector3(cell.x, cell.y, 0), new Vector3(targetCell.x, targetCell.y, 0));

                    for (int i = 0; i < candArray.Length; i++)
                    {
                        var cand = candArray[i];
                        if (!tilemap.ContainsKey(cand)) continue;
                        if (occupied.Contains(cand)) continue;
                        if (reservedSteps.Contains(cand)) continue;
                        if (reservedTargets.Contains(cand) && cand != targetCell) continue;
                        if (!integration.TryGetValue(cand, out var candCost)) continue;
                        if (candCost != currentCost) continue;

                        var d = Vector3.Distance(new Vector3(cand.x, cand.y, 0), new Vector3(targetCell.x, targetCell.y, 0));
                        if (d + 0.001f < curD)
                        {
                            bool otherPlansToOurCell = intendedSteps.Values.Any(s => s == cell);
                            if (otherPlansToOurCell) continue;

                            if (d < bestDist)
                            {
                                bestDist = d;
                                bestEqual = cand;
                                foundEqual = true;
                            }
                        }
                    }

                    if (foundEqual)
                    {
                        chosen = bestEqual;
                        found = true;
                    }
                }

                if (!found)
                {
                    unit.ReplaceWaitTimer(WaitSeconds);
                    unit.isMoving = false;
                    continue;
                }

                reservedSteps.Add(chosen);
                intendedSteps[unit] = chosen;

                var targetPos = tilemap[chosen];
                var currentPos = unit.transform.Value.position;
                var dirVec = (targetPos - currentPos);
                var dist = dirVec.magnitude;
                if (dist > 0f) dirVec /= dist;
                var speed = unit.movementSpeed.Value * _timeService.DeltaTime;
                var newPos = currentPos + dirVec * speed;

                unit.transform.Value.position = newPos;
                unit.isMoving = true;

                if (Vector3.Distance(newPos, targetPos) < ArriveThreshold)
                {
                    occupied.Remove(cell);
                    occupied.Add(chosen);
                    unit.ReplaceCurrentCell(chosen);

                    if (chosen == targetCell)
                    {
                        unit.isMoving = false;
                        unit.RemoveTargetCell();
                    }
                }
            }
        }

        private Vector3Int FindBestTargetAroundGoals_NoAlloc(Vector3Int fromCell,
                                                             Dictionary<Vector3Int, int> integration,
                                                             List<Vector3Int> goals,
                                                             Dictionary<Vector3Int, Vector3> tilemap,
                                                             HashSet<Vector3Int> occupied,
                                                             HashSet<Vector3Int> reservedTargets)
        {
            Vector3Int best = fromCell;
            int bestCost = int.MaxValue;
            int bestManh = int.MaxValue;

            foreach (var g in goals)
            {

                TryConsider(g);

                foreach (var n in _targetService.GetCardinalNeighbors(g))
                    TryConsider(n);
            }

            void TryConsider(Vector3Int c)
            {
                if (!tilemap.ContainsKey(c)) return;
                if (occupied.Contains(c)) return;
                if (reservedTargets.Contains(c)) return;
                if (!integration.TryGetValue(c, out var cost)) return;

                var manh = Math.Abs(c.x - fromCell.x) + Math.Abs(c.y - fromCell.y);
                if (cost < bestCost || (cost == bestCost && manh < bestManh))
                {
                    bestCost = cost;
                    best = c;
                    bestManh = manh;
                }
            }

            return best;
        }
    }
}