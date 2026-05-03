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

        private readonly List<GameEntity> _buffer = new(86);

        public SelectTargetCellSystem(GameContext context, TargetService targetService)
        {
            _targetService = targetService;

            _units = context.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.Transform,
                GameMatcher.CurrentCell,
                GameMatcher.Id,
                GameMatcher.UnitSize).NoneOf(GameMatcher.Moving));

            _maps = context.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.FlowFields,
                GameMatcher.IntegrationFields,
                GameMatcher.OccupField,
                GameMatcher.ReservedField,
                GameMatcher.TilemapMovement));
        }

        public void Execute()
        {
            var mapEntity = _maps.GetSingleEntity();

            if (mapEntity == null) 
                return;

            var allFlows = mapEntity.flowFields.Value;
            var allIntegrations = mapEntity.integrationFields.Value;

            foreach (var unit in _units.GetEntities(_buffer))
            {
                var cell = unit.currentCell.Value;
                var size = unit.unitSize.Value;
                var unitId = unit.id.Value;

                if (!allIntegrations.TryGetValue(size, out var integration) || !allFlows.TryGetValue(size, out var flow))
                    continue;

                if (!integration.TryGetValue(cell, out var currentCost) || currentCost == 0)
                {
                    if (unit.hasTargetCell) 
                        unit.RemoveTargetCell();

                    continue;
                }

                if (!flow.TryGetValue(cell, out var idealDir) || idealDir == Vector3Int.zero)
                {
                    if (unit.hasTargetCell) 
                        unit.RemoveTargetCell();

                    continue;
                }

                var idealStep = cell + idealDir;
                var chosen = cell;
                var found = false;

                if (CanFit(idealStep, size, unitId, mapEntity))
                {
                    chosen = idealStep;
                    found = true;
                }

                if (!found)
                {
                    var bestCost = currentCost;

                    foreach (var cand in _targetService.GetNeighbors(cell))
                    {
                        if (!CanFit(cand, size, unitId, mapEntity))
                            continue;

                        if (IsCuttingCorner(cell, cand, size, unitId, mapEntity))
                            continue;

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
                    unit.ReplaceTargetCell(chosen);
                else if (unit.hasTargetCell)
                     unit.RemoveTargetCell();
            }
        }

        private bool CanFit(Vector3Int origin, Vector2Int size, int unitId, GameEntity map)
        {
            var tilemap = map.tilemapMovement.Value;
            var occupField = map.occupField.Value;
            var reservedField = map.reservedField.Value;

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    var checkPos = new Vector3Int(origin.x + x, origin.y + y, 0);

                    if (!tilemap.ContainsKey(checkPos))
                        return false;

                    if (occupField.TryGetValue(checkPos, out var occId) && occId != unitId)
                        return false;

                    if (reservedField.TryGetValue(checkPos, out var resId) && resId != unitId)
                        return false;
                }
            }

            return true;
        }

        private bool IsCuttingCorner(Vector3Int current, Vector3Int neighbor, Vector2Int size, int unitId, GameEntity map)
        {
            if (current.x != neighbor.x && current.y != neighbor.y)
            {
                var corner1 = new Vector3Int(neighbor.x, current.y, 0);
                var corner2 = new Vector3Int(current.x, neighbor.y, 0);

                if (!CanFit(corner1, size, unitId, map) || !CanFit(corner2, size, unitId, map))
                    return true;
            }

            return false;
        }
    }
}