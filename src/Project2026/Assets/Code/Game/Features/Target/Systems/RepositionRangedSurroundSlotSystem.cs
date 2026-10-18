using Code.Game.Common.Entity;
using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Target.Systems
{
    public class RepositionRangedSurroundSlotSystem : IExecuteSystem
    {
        private readonly TargetService _targetService;

        private readonly IGroup<GameEntity> _units;
        private readonly IGroup<GameEntity> _maps;

        private readonly List<GameEntity> _buffer = new(128);

        public RepositionRangedSurroundSlotSystem(TargetService targetService)
        {
            var gameContext = Contexts.sharedInstance.game;

            _targetService = targetService;

            _units = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.RangeAttack,
                    GameMatcher.SurroundSlot,
                    GameMatcher.SurroundTargetId,
                    GameMatcher.Range,
                    GameMatcher.CurrentCell,
                    GameMatcher.UnitSize,
                    GameMatcher.Id)
                .NoneOf(
                    GameMatcher.Dead,
                    GameMatcher.Attacking));

            _maps = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.TilemapMovement,
                    GameMatcher.SurroundField,
                    GameMatcher.OccupField,
                    GameMatcher.ReservedField));
        }

        public void Execute()
        {
            var mapEntity = _maps.GetSingleEntity();

            if (mapEntity == null)
                return;

            var tilemap = mapEntity.tilemapMovement.Value;
            var surroundField = mapEntity.surroundField.Value;
            var units = _units.GetEntities(_buffer);

            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                var target = GetGameEntityById.Get(unit.surroundTargetId.Value);

                if (target == null || target.isDead || !target.hasCurrentCell)
                    continue;

                var currentCell = unit.currentCell.Value;
                var currentSlot = unit.surroundSlot.Value;

                if (currentCell != currentSlot
                    && TryHoldCurrentFiringCell(unit, target, currentCell, currentSlot, mapEntity, tilemap, surroundField))
                    continue;

                if (unit.isAttackAvailable)
                    continue;

                if (unit.currentCell.Value != unit.surroundSlot.Value)
                    continue;

                currentSlot = unit.surroundSlot.Value;

                TargetService.GetFootprint(target, out var minX, out var minY, out var maxX, out var maxY);

                var size = unit.unitSize.Value;
                var currentRing = TargetService.GetFootprintRing(currentSlot, size, minX, minY, maxX, maxY);
                var insideMinRing = currentRing < TargetService.GetRangedMinRing(unit.range.Value);

                if (!insideMinRing && !TargetService.IsTooCloseForRanged(unit, target, tilemap))
                    continue;

                if (!tilemap.TryGetValue(currentSlot, out var currentSlotWorld))
                    continue;

                var currentSqrDist = TargetService.GetSqrDistanceToTarget(currentSlotWorld, target, tilemap);

                if (!_targetService.TryPickSurroundSlot(
                        unit.currentCell.Value,
                        target,
                        unit.range.Value,
                        unit.unitSize.Value,
                        unit.id.Value,
                        mapEntity,
                        tilemap,
                        surroundField,
                        out var newSlot,
                        preferMaxRange: true))
                    continue;

                if (newSlot == currentSlot)
                    continue;

                if (!tilemap.TryGetValue(newSlot, out var newSlotWorld))
                    continue;

                var newSqrDist = TargetService.GetSqrDistanceToTarget(newSlotWorld, target, tilemap);

                if (newSqrDist <= currentSqrDist)
                    continue;

                if (!TargetService.CanFireFromCell(newSlot, target, unit.range.Value, size, tilemap))
                    continue;

                surroundField.Remove(currentSlot);
                surroundField[newSlot] = unit.id.Value;
                unit.ReplaceSurroundSlot(newSlot);
            }
        }

        private static bool TryHoldCurrentFiringCell(
            GameEntity unit,
            GameEntity target,
            Vector3Int currentCell,
            Vector3Int currentSlot,
            GameEntity mapEntity,
            Dictionary<Vector3Int, Vector3> tilemap,
            Dictionary<Vector3Int, int> surroundField)
        {
            if (!tilemap.TryGetValue(currentCell, out var currentWorld)
                || !tilemap.TryGetValue(currentSlot, out var slotWorld))
                return false;

            var currentSqrDist = TargetService.GetSqrDistanceToTarget(currentWorld, target, tilemap);
            var slotSqrDist = TargetService.GetSqrDistanceToTarget(slotWorld, target, tilemap);

            if (slotSqrDist >= currentSqrDist)
                return false;

            if (!TargetService.CanFireFromCell(
                    currentCell,
                    target,
                    unit.range.Value,
                    unit.unitSize.Value,
                    tilemap))
                return false;

            if (!TargetService.CanFitSlot(currentCell, unit.unitSize.Value, unit.id.Value, mapEntity))
                return false;

            var unitId = unit.id.Value;

            if (surroundField.TryGetValue(currentSlot, out var ownerId) && ownerId == unitId)
                surroundField.Remove(currentSlot);

            surroundField[currentCell] = unitId;
            unit.ReplaceSurroundSlot(currentCell);
            return true;
        }
    }
}
