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
        private readonly IGroup<GameEntity> _units;
        private readonly IGroup<GameEntity> _maps;

        private const float ArriveThreshold = 0.1f;
        private const float WaitSeconds = 1f;

        public MovementSystem(GameContext context, ITimeService timeService)
        {
            _timeService = timeService;
            _units = context.GetGroup(GameMatcher.AllOf(GameMatcher.Transform, GameMatcher.MovementSpeed, GameMatcher.CurrentCell));
            _maps = context.GetGroup(GameMatcher.AllOf(GameMatcher.FlowField, GameMatcher.IntegrationField, GameMatcher.TilemapMovement));
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();
            var flow = map.flowField.Value;             // Dictionary<Vector3Int, Vector3Int>
            var integration = map.integrationField.Value; // Dictionary<Vector3Int, int>
            var tilemap = map.tilemapMovement.Value;    // Dictionary<Vector3Int, Vector3>
            var units = _units.GetEntities();

            var occupied = new HashSet<Vector3Int>();
            foreach (var u in units) occupied.Add(u.currentCell.Value);

            foreach (var unit in units)
            {
                if (unit.hasWaitTimer)
                {
                    unit.ReplaceWaitTimer(unit.waitTimer.Value - _timeService.DeltaTime);
                    if (unit.waitTimer.Value <= 0) unit.RemoveWaitTimer();
                    continue;
                }

                var cell = unit.currentCell.Value;

                // если нет направления — стоим
                if (!flow.TryGetValue(cell, out var dir) || dir == Vector3Int.zero)
                {
                    unit.isMoving = false;
                    continue;
                }

                // кандидаты: вперёд по flow + соседи
                var candidates = new[]
                {
                cell + dir,
                cell + new Vector3Int(-dir.y, dir.x, 0),
                cell + new Vector3Int(dir.y, -dir.x, 0),
                cell - dir
            };

                int currentCost = integration.TryGetValue(cell, out var cc) ? cc : int.MaxValue;
                Vector3Int chosen = cell;
                int bestCost = currentCost;
                bool found = false;

                foreach (var cand in candidates)
                {
                    if (!tilemap.ContainsKey(cand)) continue;
                    if (occupied.Contains(cand)) continue;

                    int candCost = integration.TryGetValue(cand, out var cost) ? cost : int.MaxValue;

                    // выбираем клетку с минимальной стоимостью (даже если она равна или чуть хуже)
                    if (candCost < bestCost)
                    {
                        bestCost = candCost;
                        chosen = cand;
                        found = true;
                    }
                    else if (!found && candCost == bestCost)
                    {
                        // разрешаем равные — для обхода
                        chosen = cand;
                        found = true;
                    }
                }

                if (!found)
                {
                    // все клетки заняты → ждём
                    unit.ReplaceWaitTimer(WaitSeconds);
                    unit.isMoving = false;
                    continue;
                }

                // движение
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
                }
            }
        }
    }
}