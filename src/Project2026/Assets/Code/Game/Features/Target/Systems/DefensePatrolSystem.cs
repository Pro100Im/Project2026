using Code.Game.Common.Time;
using Code.Game.Features.Level;
using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Target.Systems
{
    public class DefensePatrolSystem : IExecuteSystem
    {
        private readonly ITimeService _timeService;

        private readonly IGroup<GameEntity> _castles;
        private readonly IGroup<GameEntity> _patrollers;
        private readonly IGroup<GameEntity> _patrolState;
        private readonly IGroup<GameEntity> _maps;

        private readonly List<GameEntity> _buffer = new(LevelBuffers.DefaultCapacity);

        public DefensePatrolSystem(ITimeService timeService)
        {
            var gameContext = Contexts.sharedInstance.game;

            _timeService = timeService;

            _castles = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.PlayerCastle)
                .NoneOf(
                    GameMatcher.Dead));

            _patrollers = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Team,
                    GameMatcher.GridMovement,
                    GameMatcher.MovementAvailable,
                    GameMatcher.DefensePatrolIdleDuration,
                    GameMatcher.UnitSize,
                    GameMatcher.CurrentCell)
                .NoneOf(
                    GameMatcher.Dead,
                    GameMatcher.SurroundSlot,
                    GameMatcher.Attacking,
                    GameMatcher.RallyToCastle));

            _patrolState = gameContext.GetGroup(GameMatcher
                .AnyOf(
                    GameMatcher.DefensePatrolWait,
                    GameMatcher.DefensePatrolCell));

            _maps = gameContext.GetGroup(GameMatcher.Arena);
        }

        public void Execute()
        {
            var mapEntity = _maps.GetSingleEntity();
            var castle = _castles.GetSingleEntity();

            if (mapEntity == null)
                return;

            var arena = mapEntity.arena.Value;

            LevelBuffers.Fit(_buffer, arena);

            var castleThreatened = castle != null && castle.isCastleUnderAttack;
            ClearInterruptedPatrol(castleThreatened);

            if (castleThreatened)
                return;

            var patrollers = _patrollers.GetEntities(_buffer);

            for (var i = 0; i < patrollers.Count; i++)
            {
                var unit = patrollers[i];

                if (unit.team.Value != Team.Player)
                    continue;

                var cell = unit.currentCell.Value;
                var size = unit.unitSize.Value;
                var unitId = unit.id.Value;
                var idleDuration = unit.defensePatrolIdleDuration.Value;
                var fieldOffset = DefenseFieldOffset(arena, unit);

                if (unit.hasDefensePatrolCell)
                {
                    if (cell == unit.defensePatrolCell.Value && !unit.isMoving)
                    {
                        unit.RemoveDefensePatrolCell();
                        unit.ReplaceDefensePatrolWait(idleDuration);
                    }

                    continue;
                }

                if (!IsOnDefenseGoal(arena, fieldOffset, cell))
                    continue;

                if (!unit.hasDefensePatrolWait)
                {
                    unit.AddDefensePatrolWait(idleDuration);
                    continue;
                }

                var wait = unit.defensePatrolWait.Value - _timeService.DeltaTime;

                if (wait > 0f)
                {
                    unit.ReplaceDefensePatrolWait(wait);
                    continue;
                }

                unit.RemoveDefensePatrolWait();

                if (!TryPickPatrolCell(arena, fieldOffset, cell, size, unitId, out var patrolCell))
                {
                    unit.AddDefensePatrolWait(idleDuration);
                    continue;
                }

                unit.AddDefensePatrolCell(patrolCell);
            }
        }

        private void ClearInterruptedPatrol(bool castleThreatened)
        {
            var entities = _patrolState.GetEntities(_buffer);

            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];

                if (castleThreatened
                    || entity.isRallyToCastle
                    || entity.hasSurroundSlot
                    || entity.isAttacking
                    || entity.isDead)
                {
                    if (entity.hasDefensePatrolWait)
                        entity.RemoveDefensePatrolWait();

                    if (entity.hasDefensePatrolCell)
                        entity.RemoveDefensePatrolCell();
                }
            }
        }

        /// <summary>
        /// Offset of the defense field the unit walks: the one of its own clearance class while it is in the
        /// simulation, otherwise the one that fits its footprint.
        /// </summary>
        private static int DefenseFieldOffset(LevelArena arena, GameEntity unit)
        {
            int clearance;

            if (unit.hasSimSlot)
            {
                clearance = arena.UnitClearance[unit.simSlot.Value];
            }
            else
            {
                var size = unit.unitSize.Value;
                clearance = Mathf.Clamp(Mathf.Max(size.x, size.y), 1, LevelArena.ClearanceClasses);
            }

            clearance = Mathf.Clamp(clearance, 1, LevelArena.ClearanceClasses);

            return LevelArena.FieldSlot(NavField.DefensePost, clearance) * arena.CellCount;
        }

        /// <summary>A defense post is a footprint position the field was seeded from: cost 0.</summary>
        private static bool IsOnDefenseGoal(LevelArena arena, int fieldOffset, Vector3Int cell)
        {
            var index = arena.IndexOf(cell.x, cell.y);

            return index >= 0 && arena.Integration[fieldOffset + index] == 0;
        }

        /// <summary>Uniform random pick among free defense posts, without collecting them first.</summary>
        private static bool TryPickPatrolCell(
            LevelArena arena,
            int fieldOffset,
            Vector3Int currentCell,
            Vector2Int size,
            int unitId,
            out Vector3Int patrolCell)
        {
            patrolCell = default;

            var currentIndex = arena.IndexOf(currentCell.x, currentCell.y);
            var count = 0;

            for (var index = 0; index < arena.CellCount; index++)
            {
                if (index == currentIndex || arena.Integration[fieldOffset + index] != 0)
                    continue;

                var cell = arena.CellOf(index, currentCell.z);

                if (!TargetService.CanFitSlot(cell, size, unitId, arena))
                    continue;

                count++;

                if (Random.Range(0, count) == 0)
                    patrolCell = cell;
            }

            return count > 0;
        }
    }
}
