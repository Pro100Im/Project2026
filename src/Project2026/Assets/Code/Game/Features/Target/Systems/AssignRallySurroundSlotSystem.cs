using Code.Game.Common.Entity;
using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Target.Systems
{
    public class AssignRallySurroundSlotSystem : IExecuteSystem
    {
        private readonly TargetService _targetService;

        private readonly IGroup<GameEntity> _castles;
        private readonly IGroup<GameEntity> _enemies;
        private readonly IGroup<GameEntity> _rallyingDefenders;
        private readonly IGroup<GameEntity> _maps;

        private readonly List<GameEntity> _buffer = new(256);
        private readonly List<GameEntity> _castleThreatEnemies = new(32);

        public AssignRallySurroundSlotSystem(GameContext context, TargetService targetService)
        {
            _targetService = targetService;

            _castles = context.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.PlayerCastle,
                    GameMatcher.Id)
                .NoneOf(
                    GameMatcher.Dead));

            _enemies = context.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Team,
                    GameMatcher.Id)
                .NoneOf(
                    GameMatcher.Dead));

            _rallyingDefenders = context.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.RallyToCastle,
                    GameMatcher.GridMovement,
                    GameMatcher.CurrentCell,
                    GameMatcher.Team,
                    GameMatcher.Range,
                    GameMatcher.UnitSize,
                    GameMatcher.MovementAvailable,
                    GameMatcher.Id)
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
            var castle = _castles.GetSingleEntity();

            if (mapEntity == null || castle == null)
                return;

            var castleId = castle.id.Value;
            var tilemap = mapEntity.tilemapMovement.Value;
            var surroundField = mapEntity.surroundField.Value;

            _castleThreatEnemies.Clear();
            var enemies = _enemies.GetEntities(_buffer);

            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];

                if (enemy.team.Value != Team.Enemy)
                    continue;

                if (enemy.hasSurroundTargetId && enemy.surroundTargetId.Value == castleId)
                {
                    _castleThreatEnemies.Add(enemy);
                    continue;
                }

                if (enemy.hasTargetId && enemy.targetId.Value == castleId)
                    _castleThreatEnemies.Add(enemy);
            }

            if (_castleThreatEnemies.Count == 0)
                return;

            var defenders = _rallyingDefenders.GetEntities(_buffer);

            for (var i = 0; i < defenders.Count; i++)
            {
                var defender = defenders[i];

                if (defender.team.Value != Team.Player)
                    continue;

                var defenderCell = defender.currentCell.Value;
                var defenderId = defender.id.Value;
                var size = defender.unitSize.Value;
                var range = defender.range.Value;

                GameEntity bestEnemy = null;
                var bestSqrDist = int.MaxValue;

                for (var e = 0; e < _castleThreatEnemies.Count; e++)
                {
                    var enemy = _castleThreatEnemies[e];

                    if (!enemy.hasCurrentCell)
                        continue;

                    var enemyCell = enemy.currentCell.Value;
                    var dx = enemyCell.x - defenderCell.x;
                    var dy = enemyCell.y - defenderCell.y;
                    var sqrDist = (dx * dx) + (dy * dy);

                    if (sqrDist >= bestSqrDist)
                        continue;

                    bestSqrDist = sqrDist;
                    bestEnemy = enemy;
                }

                if (bestEnemy == null)
                    continue;

                if (!_targetService.TryPickSurroundSlot(
                        defenderCell,
                        bestEnemy,
                        range,
                        size,
                        defenderId,
                        mapEntity,
                        tilemap,
                        surroundField,
                        out var slot))
                    continue;

                surroundField[slot] = defenderId;
                defender.AddSurroundSlot(slot);
                defender.AddSurroundTargetId(bestEnemy.id.Value);
                defender.isRallyToCastle = false;
            }
        }
    }
}
