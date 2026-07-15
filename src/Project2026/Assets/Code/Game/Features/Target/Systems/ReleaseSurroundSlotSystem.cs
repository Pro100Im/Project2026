using Code.Game.Common.Entity;
using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Target.Systems
{
    public class ReleaseSurroundSlotSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _units;
        private readonly IGroup<GameEntity> _maps;

        private readonly List<GameEntity> _buffer = new(256);

        public ReleaseSurroundSlotSystem(GameContext context)
        {
            _units = context.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.SurroundSlot,
                    GameMatcher.SurroundTargetId,
                    GameMatcher.Range,
                    GameMatcher.Id));

            _maps = context.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.SurroundField,
                    GameMatcher.TilemapMovement));
        }

        public void Execute()
        {
            var mapEntity = _maps.GetSingleEntity();

            if (mapEntity == null)
                return;

            var surroundField = mapEntity.surroundField.Value;
            var tilemap = mapEntity.tilemapMovement.Value;
            var units = _units.GetEntities(_buffer);

            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                var shouldRelease = unit.isDead;

                if (!shouldRelease)
                {
                    var target = GetGameEntityById.Get(unit.surroundTargetId.Value);

                    if (target == null || target.isDead)
                    {
                        shouldRelease = true;
                    }
                    else if (!IsSlotInAttackRange(unit, target, tilemap))
                    {
                        shouldRelease = true;
                    }
                }

                if (!shouldRelease)
                    continue;

                var slot = unit.surroundSlot.Value;
                surroundField.Remove(slot);

                unit.RemoveSurroundSlot();
                unit.RemoveSurroundTargetId();

                if (unit.hasTargetId)
                    unit.RemoveTargetId();
            }
        }

        private static bool IsSlotInAttackRange(
            GameEntity unit,
            GameEntity target,
            Dictionary<Vector3Int, Vector3> tilemap)
        {
            var slot = unit.surroundSlot.Value;

            if (!tilemap.TryGetValue(slot, out var slotWorld))
                return false;

            if (unit.hasUnitAnchorPoint)
                slotWorld += unit.unitAnchorPoint.Value;

            var closest = TargetService.GetClosestPoint(target, slotWorld);
            var dx = slotWorld.x - closest.x;
            var dy = slotWorld.y - closest.y;
            var physicalRange = TargetService.GetPhysicalRange(unit.range.Value);

            return (dx * dx) + (dy * dy) <= physicalRange * physicalRange;
        }
    }
}
