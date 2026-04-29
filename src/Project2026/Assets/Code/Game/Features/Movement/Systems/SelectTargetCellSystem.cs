using Code.Game.Common.Time;
using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Movement.Systems
{
    public class SelectTargetCellSystem : IExecuteSystem
    {
        private readonly ITimeService _timeService;
        private readonly TargetService _targetService;
        private readonly IGroup<GameEntity> _units;
        private readonly IGroup<GameEntity> _maps;

        private const float WaitSeconds = 0.5f;

        public SelectTargetCellSystem(GameContext context, ITimeService timeService, TargetService targetService)
        {
            _timeService = timeService;
            _targetService = targetService;

            _units = context.GetGroup(GameMatcher.AllOf(
                GameMatcher.Transform,
                GameMatcher.CurrentCell));

            _maps = context.GetGroup(GameMatcher.AllOf(
                GameMatcher.FlowField,
                GameMatcher.IntegrationField,
                GameMatcher.TilemapMovement,
                GameMatcher.OccupField,
                GameMatcher.ReservedField));
        }

        public void Execute()
        {
            var mapEntity = _maps.GetSingleEntity();
            var flow = mapEntity.flowField.Value;
            var integration = mapEntity.integrationField.Value;
            var tilemap = mapEntity.tilemapMovement.Value;
            var occupied = mapEntity.occupField.Value;
            var reserved = mapEntity.reservedField.Value;

            foreach (var unit in _units.GetEntities())
            {
                if (unit.isMoving) continue;

                if (unit.hasWaitTimer)
                {
                    unit.ReplaceWaitTimer(unit.waitTimer.Value - _timeService.DeltaTime);
                    if (unit.waitTimer.Value <= 0) unit.RemoveWaitTimer();
                    continue;
                }

                var cell = unit.currentCell.Value;

                if (!integration.TryGetValue(cell, out var currentCost)) 
                    continue;

                if (currentCost == 0)
                {
                    if (unit.hasTargetCell) unit.RemoveTargetCell();
                    continue;
                }

                if (!flow.TryGetValue(cell, out var idealDir) || idealDir == Vector3Int.zero)
                    continue;

                var idealStep = cell + idealDir;
                var chosen = cell;
                var found = false;

                if (tilemap.ContainsKey(idealStep) && !occupied.ContainsKey(idealStep) && (!reserved.TryGetValue(idealStep, out var rId) || rId == unit.id.Value))
                {
                    chosen = idealStep;
                    found = true;
                }

                if (!found)
                {
                    var bestCost = currentCost;

                    foreach (var cand in _targetService.GetNeighbors(cell))
                    {
                        if (!tilemap.ContainsKey(cand) || occupied.ContainsKey(cand)) 
                            continue;

                        if (reserved.TryGetValue(cand, out var resId) && resId != unit.id.Value) 
                            continue;

                        if (IsCuttingCorner(cell, cand, tilemap)) 
                            continue;

                        if (integration.TryGetValue(cand, out var candCost))
                        {
                            if (candCost < bestCost)
                            {
                                bestCost = candCost;
                                chosen = cand;
                                found = true;
                            }
                        }
                    }
                }

                if (found)
                {
                    unit.ReplaceTargetCell(chosen);
                    //reserved[chosen] = unit.id.Value;
                }
                else
                {
                    unit.ReplaceTargetCell(cell);
                    unit.ReplaceWaitTimer(WaitSeconds);
                }
            }
        }

        private bool IsCuttingCorner(Vector3Int current, Vector3Int neighbor, Dictionary<Vector3Int, Vector3> tilemap)
        {
            if (current.x != neighbor.x && current.y != neighbor.y)
            {
                var corner1 = new Vector3Int(neighbor.x, current.y, 0);
                var corner2 = new Vector3Int(current.x, neighbor.y, 0);

                if (!tilemap.ContainsKey(corner1) || !tilemap.ContainsKey(corner2))
                    return true;
            }

            return false;
        }
    }
}