using Code.Game.Common.Time;
using Entitas;
using UnityEngine;

namespace Code.Game.Features.Movement.Systems
{
    public class SelectTargetCellSystem : IExecuteSystem
    {
        private readonly ITimeService _timeService;

        private readonly IGroup<GameEntity> _units;
        private readonly IGroup<GameEntity> _maps;

        private const float WaitSeconds = 1f;

        public SelectTargetCellSystem(GameContext context, ITimeService timeService)
        {
            _timeService = timeService;

            _units = context.GetGroup(GameMatcher.AllOf(
                GameMatcher.Transform,
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
            var reserved = map.reservedField.Value;

            var units = _units.GetEntities();

            foreach (var unit in units)
            {
                if (unit.isMoving)
                    continue;

                if (unit.hasWaitTimer)
                {
                    unit.ReplaceWaitTimer(unit.waitTimer.Value - _timeService.DeltaTime);
                    if (unit.waitTimer.Value <= 0)
                        unit.RemoveWaitTimer();

                    continue;
                }

                var cell = unit.currentCell.Value;

                if (integration.TryGetValue(cell, out var currentCellCost) && currentCellCost == 0)
                {
                    if (unit.hasTargetCell) unit.RemoveTargetCell();

                    continue;
                }


                if (!flow.TryGetValue(cell, out var dir) || dir == Vector3Int.zero)
                    continue;

                var candidates = new[] {
                cell + dir,
                cell + new Vector3Int(-dir.y, dir.x, 0),
                cell + new Vector3Int(dir.y, -dir.x, 0),
                cell - dir
            };

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

                    if (reserved.ContainsKey(cand))
                        continue;

                    if (reserved.TryGetValue(cand, out var resId) && resId != unit.id.Value)
                        continue;

                    var candCost = integration.TryGetValue(cand, out var cost) ? cost : int.MaxValue;
                    var candDir = cand - cell;

                    if (candDir == -lastDir)
                        continue;

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

                    unit.ReplaceTargetCell(unit.currentCell.Value);

                    continue;
                }

                unit.ReplaceTargetCell(chosen);
                reserved[chosen] = unit.id.Value;
            }
        }
    }
}