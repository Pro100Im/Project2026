using Code.Game.Common.Time;
using Code.Game.Features.Level;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Movement.Systems
{
    /// <summary>
    /// Bridge from the arena back to entities after a simulation step: logical position, the cell the unit
    /// stands in, the moving flag and the facing. Components are replaced only when the value changed.
    /// </summary>
    public class ApplySimUnitsSystem : IExecuteSystem
    {
        private const float FlipSpeedRatio = 0.35f;
        private const float FlipInterval = 0.25f;

        private readonly ITimeService _timeService;
        private readonly IGroup<GameEntity> _maps;
        private readonly IGroup<GameEntity> _simUnits;

        private readonly List<GameEntity> _buffer = new(1024);

        public ApplySimUnitsSystem(ITimeService timeService)
        {
            var gameContext = Contexts.sharedInstance.game;

            _timeService = timeService;
            _maps = gameContext.GetGroup(GameMatcher.Arena);

            _simUnits = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.SimSlot,
                    GameMatcher.WoldPos,
                    GameMatcher.CurrentCell,
                    GameMatcher.UnitSize));
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();

            if (map == null)
                return;

            var arena = map.arena.Value;
            var deltaTime = _timeService.DeltaTime;
            var units = _simUnits.GetEntities(_buffer);

            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                var slot = unit.simSlot.Value;

                if (arena.UnitAlive[slot] == 0)
                    continue;

                ApplyPosition(arena, unit, slot);
                ApplyMoving(arena, unit, slot, deltaTime);

                // The unit could not reach its patrol point and stopped to think: let it pick another one.
                if (arena.UnitRethinks[slot] != 0)
                {
                    arena.UnitRethinks[slot] = 0;

                    if (unit.hasDefensePatrolCell)
                        unit.RemoveDefensePatrolCell();
                }
            }
        }

        private static void ApplyPosition(LevelArena arena, GameEntity unit, int slot)
        {
            var position = arena.UnitPos[slot];
            var offset = arena.UnitViewOffset[slot];
            var current = unit.woldPos.Value;
            var world = new Vector3(position.x + offset.x, position.y + offset.y, current.z);

            if (world != current)
                unit.ReplaceWoldPos(world);

            // CurrentCell is the origin (bottom-left) cell of the footprint, as before.
            var cellSize = arena.CellSize;
            var size = unit.unitSize.Value;
            var oldCell = unit.currentCell.Value;

            var cell = new Vector3Int(
                arena.OriginX + Mathf.FloorToInt((position.x - ((size.x - 1) * cellSize * 0.5f) - arena.WorldMin.x) / cellSize),
                arena.OriginY + Mathf.FloorToInt((position.y - ((size.y - 1) * cellSize * 0.5f) - arena.WorldMin.y) / cellSize),
                oldCell.z);

            if (cell != oldCell)
                unit.ReplaceCurrentCell(cell);
        }

        private static void ApplyMoving(LevelArena arena, GameEntity unit, int slot, float deltaTime)
        {
            var moving = arena.UnitMoving[slot] != 0 && !unit.isAttacking && !unit.isFreezeDebuff;

            if (unit.isMoving != moving)
                unit.isMoving = moving;

            if (arena.UnitFlipTimer[slot] > 0f)
                arena.UnitFlipTimer[slot] -= deltaTime;

            if (!moving || !unit.hasSpriteRenderer || arena.UnitFlipTimer[slot] > 0f)
                return;

            // Facing follows the velocity, with a dead zone and a minimum time between flips.
            var velocityX = arena.UnitVel[slot].x;

            if (Mathf.Abs(velocityX) < arena.UnitSpeed[slot] * FlipSpeedRatio)
                return;

            var flipX = velocityX < 0f;
            var spriteRenderer = unit.spriteRenderer.Value;

            if (spriteRenderer.flipX == flipX)
                return;

            spriteRenderer.flipX = flipX;
            arena.UnitFlipTimer[slot] = FlipInterval;
        }
    }
}
