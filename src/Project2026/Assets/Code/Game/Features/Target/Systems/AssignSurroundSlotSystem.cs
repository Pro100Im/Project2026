using Code.Game.Common.Entity;
using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Target.Systems
{
    public class AssignSurroundSlotSystem : IExecuteSystem
    {
        private readonly TargetService _targetService;

        private readonly IGroup<GameEntity> _units;
        private readonly IGroup<GameEntity> _maps;

        private readonly List<GameEntity> _buffer = new(256);

        public AssignSurroundSlotSystem(GameContext context, TargetService targetService)
        {
            _targetService = targetService;

            _units = context.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.GridMovement,
                    GameMatcher.CurrentCell,
                    GameMatcher.Transform,
                    GameMatcher.Id,
                    GameMatcher.Team,
                    GameMatcher.Range,
                    GameMatcher.UnitSize,
                    GameMatcher.MovementAvailable,
                    GameMatcher.TargetId)
                .NoneOf(
                    GameMatcher.SurroundSlot,
                    GameMatcher.Dead));

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
                var targetId = unit.targetId.Value;
                var target = GetGameEntityById.Get(targetId);

                if (target == null
                    || target.isDead
                    || !target.hasCurrentCell
                    || !target.isTargetable
                    || target.team.Value == unit.team.Value)
                    continue;

                TargetService.GetFootprint(target, out var minX, out var minY, out var maxX, out var maxY);

                if (!_targetService.TryPickSurroundSlot(
                        unit.currentCell.Value,
                        target,
                        minX,
                        minY,
                        maxX,
                        maxY,
                        unit.range.Value,
                        unit.unitSize.Value,
                        unit.id.Value,
                        mapEntity,
                        tilemap,
                        surroundField,
                        out var slot,
                        unit.isRangeAttack))
                    continue;

                surroundField[slot] = unit.id.Value;
                unit.AddSurroundSlot(slot);
                unit.AddSurroundTargetId(targetId);
            }
        }
    }
}
