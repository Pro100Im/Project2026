using Code.Game.Common.Time;
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

        private readonly List<GameEntity> _buffer = new(256);
        private readonly List<Vector3Int> _candidates = new(64);

        public DefensePatrolSystem(GameContext context, ITimeService timeService)
        {
            _timeService = timeService;

            _castles = context.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.PlayerCastle)
                .NoneOf(
                    GameMatcher.Dead));

            _patrollers = context.GetGroup(GameMatcher
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

            _patrolState = context.GetGroup(GameMatcher
                .AnyOf(
                    GameMatcher.DefensePatrolWait,
                    GameMatcher.DefensePatrolCell));

            _maps = context.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.DefenseIntegrationFields,
                    GameMatcher.OccupField,
                    GameMatcher.ReservedField,
                    GameMatcher.TilemapMovement));
        }

        public void Execute()
        {
            var mapEntity = _maps.GetSingleEntity();
            var castle = _castles.GetSingleEntity();

            if (mapEntity == null)
                return;

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

                if (unit.hasDefensePatrolCell)
                {
                    if (cell == unit.defensePatrolCell.Value && !unit.isMoving)
                    {
                        unit.RemoveDefensePatrolCell();
                        unit.ReplaceDefensePatrolWait(idleDuration);
                    }

                    continue;
                }

                if (!IsOnDefenseGoal(mapEntity, size, cell))
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

                if (!TryPickPatrolCell(unit, cell, size, unitId, mapEntity, out var patrolCell))
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

        private static bool IsOnDefenseGoal(GameEntity map, Vector2Int size, Vector3Int cell)
        {
            if (!map.defenseIntegrationFields.Value.TryGetValue(size, out var integration))
                return false;

            return integration.TryGetValue(cell, out var cost) && cost == 0;
        }

        private bool TryPickPatrolCell(
            GameEntity unit,
            Vector3Int currentCell,
            Vector2Int size,
            int unitId,
            GameEntity map,
            out Vector3Int patrolCell)
        {
            patrolCell = default;

            if (!map.defenseIntegrationFields.Value.TryGetValue(size, out var integration))
                return false;

            _candidates.Clear();

            foreach (var entry in integration)
            {
                if (entry.Value != 0 || entry.Key == currentCell)
                    continue;

                if (!TargetService.CanFitSlot(entry.Key, size, unitId, map))
                    continue;

                _candidates.Add(entry.Key);
            }

            if (_candidates.Count == 0)
                return false;

            patrolCell = _candidates[Random.Range(0, _candidates.Count)];
            return true;
        }
    }
}
