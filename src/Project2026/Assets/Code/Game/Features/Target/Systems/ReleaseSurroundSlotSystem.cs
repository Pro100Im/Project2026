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

        public ReleaseSurroundSlotSystem()
        {
            var gameContext = Contexts.sharedInstance.game;

            _units = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.SurroundSlot,
                    GameMatcher.SurroundTargetId,
                    GameMatcher.Range,
                    GameMatcher.Id,
                    GameMatcher.UnitSize));

            _maps = gameContext.GetGroup(GameMatcher
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
                var slot = unit.surroundSlot.Value;
                var unitId = unit.id.Value;
                var shouldRelease = unit.isDead;

                if (!shouldRelease)
                {
                    if (!surroundField.TryGetValue(slot, out var ownerId) || ownerId != unitId)
                    {
                        shouldRelease = true;
                    }
                    else
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
                }

                if (!shouldRelease)
                    continue;

                if (surroundField.TryGetValue(slot, out var currentOwner) && currentOwner == unitId)
                    surroundField.Remove(slot);

                var releasedTargetId = unit.surroundTargetId.Value;

                unit.RemoveSurroundSlot();
                unit.RemoveSurroundTargetId();

                var releasedTarget = GetGameEntityById.Get(releasedTargetId);
                var targetGone = releasedTarget == null || releasedTarget.isDead;

                if ((unit.isDead || targetGone)
                    && unit.hasTargetId
                    && unit.targetId.Value == releasedTargetId
                    && !unit.isAttacking)
                {
                    unit.RemoveTargetId();
                }
            }
        }

        private static bool IsSlotInAttackRange(GameEntity unit, GameEntity target, Dictionary<Vector3Int, Vector3> tilemap)
        {
            if (!target.hasCurrentCell)
                return false;

            var slot = unit.surroundSlot.Value;
            var size = unit.unitSize.Value;

            TargetService.GetFootprint(target, out var minX, out var minY, out var maxX, out var maxY);

            var ring = TargetService.GetFootprintRing(slot, size, minX, minY, maxX, maxY);
            var maxRing = TargetService.GetSurroundMaxRing(unit.range.Value);

            if (ring < 1 || ring > maxRing)
                return false;

            var bestSqr = float.MaxValue;
            var found = false;

            for (var x = 0; x < size.x; x++)
            {
                for (var y = 0; y < size.y; y++)
                {
                    var cell = new Vector3Int(slot.x + x, slot.y + y, 0);

                    if (!tilemap.TryGetValue(cell, out var cellWorld))
                        continue;

                    var closest = TargetService.GetClosestPoint(target, tilemap, cellWorld);
                    var dx = cellWorld.x - closest.x;
                    var dy = cellWorld.y - closest.y;
                    var cellSqr = (dx * dx) + (dy * dy);

                    if (cellSqr < bestSqr)
                        bestSqr = cellSqr;

                    found = true;
                }
            }

            if (!found)
                return false;

            var physicalRange = TargetService.GetPhysicalRange(unit.range.Value);

            return bestSqr <= physicalRange * physicalRange;
        }
    }
}
