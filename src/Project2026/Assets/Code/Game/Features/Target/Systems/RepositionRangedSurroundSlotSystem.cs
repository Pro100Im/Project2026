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

        public RepositionRangedSurroundSlotSystem(GameContext context, TargetService targetService)
        {
            _targetService = targetService;

            _units = context.GetGroup(GameMatcher
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

            _maps = context.GetGroup(GameMatcher
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

                if (unit.isAttackAvailable)
                    continue;

                if (unit.currentCell.Value != unit.surroundSlot.Value)
                    continue;

                var target = GetGameEntityById.Get(unit.surroundTargetId.Value);

                if (target == null || target.isDead || !target.hasCurrentCell)
                    continue;

                if (!TargetService.IsTooCloseForRanged(unit, target, tilemap))
                    continue;

                var currentSlot = unit.surroundSlot.Value;

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

                surroundField.Remove(currentSlot);
                surroundField[newSlot] = unit.id.Value;
                unit.ReplaceSurroundSlot(newSlot);
            }
        }
    }
}
