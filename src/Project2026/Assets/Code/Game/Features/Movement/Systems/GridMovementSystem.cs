using Code.Game.Common.Time;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Movement.Systems
{
    public class GridMovementSystem : IExecuteSystem
    {
        private readonly ITimeService _timeService;

        private readonly IGroup<GameEntity> _units;
        private readonly IGroup<GameEntity> _maps;

        private const float _arriveThreshold = 0.1f;
        private const float _distThreshold = 0.001f;

        private readonly List<GameEntity> _unitsBuffer = new(512);

        public GridMovementSystem(GameContext context, ITimeService timeService)
        {
            _timeService = timeService;

            _units = context.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.Transform,
                GameMatcher.MovementSpeed,
                GameMatcher.MovementSpeedBonus,
                GameMatcher.MovementOffset,
                GameMatcher.CurrentCell,
                GameMatcher.UnitSize,
                GameMatcher.GridMovement));

            _maps = context.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.FlowFields,
                GameMatcher.IntegrationFields,
                GameMatcher.TilemapMovement));
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();

            if(map == null)
                return;

            var allFlows = map.flowFields.Value;
            var tilemap = map.tilemapMovement.Value;
            var units = _units.GetEntities(_unitsBuffer);

            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];

                if (!unit.hasTargetCell || unit.isAttacking || unit.hasTargetId || unit.isDead)
                {
                    if (unit.isMoving)
                    {
                        unit.isMoving = false;

                        if (unit.hasVelocity)
                            unit.RemoveVelocity();
                    }

                    continue;
                }

                var targetCell = unit.targetCell.Value;
                var currentCell = unit.currentCell.Value;

                if (targetCell == currentCell)
                    continue;

                var movementOffset = unit.movementOffset.Value;
                var targetWorldPos = tilemap[targetCell] + movementOffset;
                var currentPos = unit.woldPos.Value;
                var dirVec = (targetWorldPos - currentPos);
                var dist = dirVec.magnitude;

                if (dist > _distThreshold)
                    dirVec /= dist;

                var speed = unit.movementSpeed.Value * unit.movementSpeedBonus.Value * _timeService.DeltaTime;
                var newPos = currentPos + dirVec * speed;

                unit.ReplaceVelocity(dirVec * unit.movementSpeed.Value);
                unit.ReplaceWoldPos(newPos);
                unit.transform.Value.position = newPos;
                unit.isMoving = true;

                if (Vector3.Distance(newPos, targetWorldPos) < _arriveThreshold)
                {
                    unit.ReplaceCurrentCell(targetCell);

                    var moveStep = targetCell - currentCell;

                    unit.ReplaceLastDirection(moveStep);
                }
            }
        }
    }
}