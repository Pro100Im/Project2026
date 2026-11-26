using Code.Game.Features.Level;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Movement.Systems
{
    /// <summary>
    /// Bridge from entities to the arena: hands out and returns sim slots and, every tick, copies what each
    /// unit wants to do (hold, walk to a point, follow a field) into the unit arrays.
    /// </summary>
    public class SyncSimUnitsSystem : IExecuteSystem
    {
        private const float ArriveRadius = 0.04f;
        private const float RadiusPerCell = 0.45f;
        private const float MinWallRadius = 0.08f;

        private readonly IGroup<GameEntity> _maps;
        private readonly IGroup<GameEntity> _newUnits;
        private readonly IGroup<GameEntity> _simUnits;

        private readonly List<GameEntity> _buffer = new(1024);

        public SyncSimUnitsSystem()
        {
            var gameContext = Contexts.sharedInstance.game;

            _maps = gameContext.GetGroup(GameMatcher.Arena);

            _newUnits = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.GridMovement,
                    GameMatcher.CurrentCell,
                    GameMatcher.UnitSize,
                    GameMatcher.MovementSpeed,
                    GameMatcher.MovementSpeedBonus,
                    GameMatcher.MovementOffset,
                    GameMatcher.Transform,
                    GameMatcher.WoldPos,
                    GameMatcher.Team,
                    GameMatcher.Id)
                .NoneOf(
                    GameMatcher.SimSlot,
                    GameMatcher.Dead,
                    GameMatcher.Destructed));

            _simUnits = gameContext.GetGroup(GameMatcher.SimSlot);
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();

            if (map == null)
                return;

            var arena = map.arena.Value;

            arena.SyncStamp++;

            var units = _simUnits.GetEntities(_buffer);

            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                var slot = unit.simSlot.Value;

                if (unit.isDead || unit.isDestructed)
                {
                    arena.ReleaseSlot(slot);
                    unit.RemoveSimSlot();
                    continue;
                }

                arena.UnitSeen[slot] = arena.SyncStamp;
                Gather(arena, unit, slot);
            }

            // Slots whose entity vanished without going through death.
            for (var slot = arena.SlotLimit - 1; slot >= 0; slot--)
            {
                if (arena.UnitAlive[slot] != 0 && arena.UnitSeen[slot] != arena.SyncStamp)
                    arena.ReleaseSlot(slot);
            }

            units = _newUnits.GetEntities(_buffer);

            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                var slot = arena.AcquireSlot();

                // Level budget is used up: the unit waits where it spawned until a slot frees.
                if (slot < 0)
                    break;

                Initialize(arena, unit, slot);
                unit.AddSimSlot(slot);
                Gather(arena, unit, slot);
            }
        }

        private static void Initialize(LevelArena arena, GameEntity unit, int slot)
        {
            var cellSize = arena.CellSize;
            var size = unit.unitSize.Value;
            var cell = unit.currentCell.Value;
            var half = new Vector2(size.x * cellSize * 0.5f, size.y * cellSize * 0.5f);
            var position = arena.CellMin(cell.x, cell.y) + half;

            var radius = unit.hasUnitRadius
                ? unit.unitRadius.Value
                : Mathf.Max(size.x, size.y) * cellSize * RadiusPerCell;

            // A body wider than the corridor it stands in collides with walls as a smaller circle,
            // otherwise it could never move.
            var wallRadius = radius;
            Vector2 settled;

            while (!CrowdCollision.TrySettle(arena, position, wallRadius, out settled) && wallRadius > MinWallRadius)
                wallRadius *= 0.8f;

            if (CrowdCollision.IsFree(arena, settled, wallRadius))
                position = settled;

            var offset = unit.movementOffset.Value;

            arena.UnitEntityId[slot] = unit.id.Value;
            arena.UnitPos[slot] = position;
            arena.UnitPrevPos[slot] = position;
            arena.UnitRadius[slot] = radius;
            arena.UnitWallRadius[slot] = wallRadius;
            arena.UnitViewOffset[slot] = new Vector2(
                offset.x - ((size.x - 1) * cellSize * 0.5f),
                offset.y - ((size.y - 1) * cellSize * 0.5f));

            // Width of the body in cells picks the navigation field: 1x1, 2x2 or 3x3 footprint.
            arena.UnitClearance[slot] = (byte)Mathf.Clamp(
                Mathf.CeilToInt((radius * 2f / cellSize) - 0.01f),
                1,
                LevelArena.ClearanceClasses);

            if (radius > arena.MaxUnitRadius)
                arena.MaxUnitRadius = radius;
        }

        private static void Gather(LevelArena arena, GameEntity unit, int slot)
        {
            var mode = UnitModes.Hold;

            arena.UnitSpeed[slot] = Mathf.Max(unit.movementSpeed.Value * unit.movementSpeedBonus.Value, 0f);

            if (!unit.isAttacking && !unit.isFreezeDebuff && unit.isMovementAvailable)
            {
                if (unit.hasSurroundSlot)
                {
                    var goal = FootprintCenter(arena, unit, unit.surroundSlot.Value);
                    var arrived = unit.currentCell.Value == unit.surroundSlot.Value
                        && (arena.UnitPos[slot] - goal).sqrMagnitude <= ArriveRadius * ArriveRadius;

                    arena.UnitGoal[slot] = goal;
                    mode = arrived ? UnitModes.Hold : UnitModes.Point;
                }
                else if (unit.hasDefensePatrolCell)
                {
                    arena.UnitGoal[slot] = FootprintCenter(arena, unit, unit.defensePatrolCell.Value);
                    mode = UnitModes.Point;
                }
                else if (unit.team.Value == Team.Enemy)
                {
                    arena.UnitField[slot] = NavField.EnemyGoal;
                    mode = UnitModes.Flow;
                }
                else if (unit.team.Value == Team.Player)
                {
                    arena.UnitField[slot] = NavField.DefensePost;
                    mode = UnitModes.Flow;
                }
            }

            arena.UnitMode[slot] = mode;
            arena.UnitImmovable[slot] = (byte)(mode == UnitModes.Hold ? 1 : 0);
        }

        private static Vector2 FootprintCenter(LevelArena arena, GameEntity unit, Vector3Int originCell)
        {
            var size = unit.unitSize.Value;

            return arena.CellMin(originCell.x, originCell.y)
                + new Vector2(size.x * arena.CellSize * 0.5f, size.y * arena.CellSize * 0.5f);
        }
    }
}
