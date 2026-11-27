using Code.Game.Common.Entity;
using Code.Game.Features.Level;
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

        private readonly List<GameEntity> _buffer = new(LevelBuffers.DefaultCapacity);

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
                var slot = unit.surroundSlot.Value;
                var unitId = unit.id.Value;
                var shouldRelease = unit.isDead;

                if (!shouldRelease)
                {
                    if (arena.SurroundOwnerAt(slot) != unitId)
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
                        else if (!IsCellInAttackRange(unit, target, arena, slot))
                        {
                            if (!TryReanchorSlot(unit, target, arena))
                                shouldRelease = true;
                        }
                    }
                }

                if (!shouldRelease)
                    continue;

                arena.ReleaseSurround(slot, unitId);

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

        private static bool TryReanchorSlot(GameEntity unit, GameEntity target, LevelArena arena)
        {
            if (!unit.hasCurrentCell)
                return false;

            var currentCell = unit.currentCell.Value;

            if (currentCell == unit.surroundSlot.Value)
                return false;

            if (!IsCellInAttackRange(unit, target, arena, currentCell))
                return false;

            var unitId = unit.id.Value;

            arena.ReleaseSurround(unit.surroundSlot.Value, unitId);
            arena.ClaimSurround(currentCell, unitId);
            unit.ReplaceSurroundSlot(currentCell);
            return true;
        }

        private static bool IsCellInAttackRange(
            GameEntity unit,
            GameEntity target,
            LevelArena arena,
            Vector3Int cell)
        {
            if (!target.hasCurrentCell)
                return false;

            var size = unit.unitSize.Value;

            TargetService.GetFootprint(target, out var minX, out var minY, out var maxX, out var maxY);

            var ring = TargetService.GetFootprintRing(cell, size, minX, minY, maxX, maxY);
            var maxRing = TargetService.GetSurroundMaxRing(unit.range.Value);

            if (ring < 1 || ring > maxRing)
                return false;

            var bestSqr = float.MaxValue;
            var found = false;

            for (var x = 0; x < size.x; x++)
            {
                for (var y = 0; y < size.y; y++)
                {
                    var footprintCell = new Vector3Int(cell.x + x, cell.y + y, 0);

                    if (!arena.TryGetCellWorld(footprintCell, out var cellWorld))
                        continue;

                    var closest = TargetService.GetClosestPoint(target, arena, cellWorld);
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
