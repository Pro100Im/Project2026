using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Target.Systems
{
    public class SelectTargetCellSystem : IExecuteSystem
    {
        private readonly TargetService _targetService;
        private readonly IGroup<GameEntity> _units;
        private readonly IGroup<GameEntity> _maps;

        private const float WaitSeconds = 0.5f;

        public SelectTargetCellSystem(GameContext context, TargetService targetService)
        {
            _targetService = targetService;

            _units = context.GetGroup(GameMatcher.AllOf(
                GameMatcher.Transform,
                GameMatcher.CurrentCell,
                GameMatcher.Id).NoneOf(GameMatcher.Moving));

            _maps = context.GetGroup(GameMatcher.AllOf(
                GameMatcher.FlowField,
                GameMatcher.IntegrationField,
                GameMatcher.TargetFlow));
        }

        public void Execute()
        {
            var mapEntity = _maps.GetSingleEntity();
            var flow = mapEntity.flowField.Value;
            var integration = mapEntity.integrationField.Value;
            var targets = mapEntity.targetFlow.Value;

            foreach (var unit in _units.GetEntities())
            {
                var cell = unit.currentCell.Value;

                if (!integration.TryGetValue(cell, out var currentCost)) 
                    continue;

                if (!flow.TryGetValue(cell, out var idealDir) || idealDir == Vector3Int.zero) 
                    continue;

                var idealStep = cell + idealDir;
                var chosen = cell;
                var found = false;

                if (IsCellAvailable(idealStep, unit.id.Value, mapEntity))
                {
                    chosen = idealStep;
                    found = true;
                }

                if (!found)
                {
                    var bestCost = currentCost;

                    foreach (var cand in _targetService.GetNeighbors(cell))
                    {
                        if (!IsCellAvailable(cand, unit.id.Value, mapEntity)) continue;
                        if (IsCuttingCorner(cell, cand, mapEntity.tilemapMovement.Value)) continue;

                        if (integration.TryGetValue(cand, out var candCost))
                        {
                            if (candCost < bestCost + 5)
                            {
                                bestCost = candCost;
                                chosen = cand;
                                found = true;
                            }
                        }
                    }
                }

                if (found && chosen != cell)
                {
                    unit.ReplaceTargetCell(chosen);
                }
                else
                {
                    unit.ReplaceWaitTimer(WaitSeconds);

                    if (unit.hasTargetCell) unit.RemoveTargetCell();
                }
            }
        }

        private bool IsCellAvailable(Vector3Int cell, int unitId, GameEntity map)
        {
            if (!map.tilemapMovement.Value.ContainsKey(cell)) 
                return false;

            if (map.occupField.Value.ContainsKey(cell)) 
                return false;

            if (map.reservedField.Value.TryGetValue(cell, out var resId) && resId != unitId) 
                return false;

            return true;
        }

        private bool IsCuttingCorner(Vector3Int current, Vector3Int neighbor, Dictionary<Vector3Int, Vector3> tilemap)
        {
            if (current.x != neighbor.x && current.y != neighbor.y)
            {
                var corner1 = new Vector3Int(neighbor.x, current.y, 0);
                var corner2 = new Vector3Int(current.x, neighbor.y, 0);

                return !tilemap.ContainsKey(corner1) || !tilemap.ContainsKey(corner2);
            }

            return false;
        }
    }
}