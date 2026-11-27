using Code.Game.Common.Entity;
using Code.Game.Features.Level;
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

        private readonly List<GameEntity> _buffer = new(LevelBuffers.DefaultCapacity);

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

            _maps = gameContext.GetGroup(GameMatcher.Arena);
        }

        public void Execute()
        {
            var mapEntity = _maps.GetSingleEntity();

            if (mapEntity == null)
                return;

            var arena = mapEntity.arena.Value;
            var units = _units.GetEntities(LevelBuffers.Fit(_buffer, arena));

            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                var target = GetGameEntityById.Get(unit.surroundTargetId.Value);

                if (target == null || target.isDead || !target.hasCurrentCell)
                    continue;

                var currentCell = unit.currentCell.Value;
                var currentSlot = unit.surroundSlot.Value;

                if (currentCell != currentSlot
                    && TryHoldCurrentFiringCell(unit, target, currentCell, currentSlot, arena))
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

                if (!insideMinRing && !TargetService.IsTooCloseForRanged(unit, target, arena))
                    continue;

                if (!arena.TryGetCellWorld(currentSlot, out var currentSlotWorld))
                    continue;

                var currentSqrDist = TargetService.GetSqrDistanceToTarget(currentSlotWorld, target, arena);

                if (!_targetService.TryPickSurroundSlot(
                        unit.currentCell.Value,
                        target,
                        unit.range.Value,
                        unit.unitSize.Value,
                        unit.id.Value,
                        arena,
                        out var newSlot,
                        preferMaxRange: true))
                    continue;

                if (newSlot == currentSlot)
                    continue;

                if (!arena.TryGetCellWorld(newSlot, out var newSlotWorld))
                    continue;

                var newSqrDist = TargetService.GetSqrDistanceToTarget(newSlotWorld, target, arena);

                if (newSqrDist <= currentSqrDist)
                    continue;

                if (!TargetService.CanFireFromCell(newSlot, target, unit.range.Value, size, arena))
                    continue;

                arena.ClearSurround(currentSlot);
                arena.ClaimSurround(newSlot, unit.id.Value);
                unit.ReplaceSurroundSlot(newSlot);
            }
        }

        private static bool TryHoldCurrentFiringCell(
            GameEntity unit,
            GameEntity target,
            Vector3Int currentCell,
            Vector3Int currentSlot,
            LevelArena arena)
        {
            if (!arena.TryGetCellWorld(currentCell, out var currentWorld)
                || !arena.TryGetCellWorld(currentSlot, out var slotWorld))
                return false;

            var currentSqrDist = TargetService.GetSqrDistanceToTarget(currentWorld, target, arena);
            var slotSqrDist = TargetService.GetSqrDistanceToTarget(slotWorld, target, arena);

            if (slotSqrDist >= currentSqrDist)
                return false;

            if (!TargetService.CanFireFromCell(
                    currentCell,
                    target,
                    unit.range.Value,
                    unit.unitSize.Value,
                    arena))
                return false;

            if (!TargetService.CanFitSlot(currentCell, unit.unitSize.Value, unit.id.Value, arena))
                return false;

            var unitId = unit.id.Value;

            arena.ReleaseSurround(currentSlot, unitId);
            arena.ClaimSurround(currentCell, unitId);
            unit.ReplaceSurroundSlot(currentCell);
            return true;
        }
    }
}
