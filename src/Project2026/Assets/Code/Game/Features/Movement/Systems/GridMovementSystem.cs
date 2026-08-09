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

        private const float _distThreshold = 0.001f;

        private readonly List<GameEntity> _unitsBuffer = new(512);

        public GridMovementSystem(ITimeService timeService)
        {
            var gameContext = Contexts.sharedInstance.game;

            _timeService = timeService;

            _units = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.Transform,
                GameMatcher.MovementSpeed,
                GameMatcher.MovementSpeedBonus,
                GameMatcher.MovementOffset,
                GameMatcher.CurrentCell,
                GameMatcher.UnitSize,
                GameMatcher.GridMovement));

            _maps = gameContext.GetGroup(GameMatcher
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

            var tilemap = map.tilemapMovement.Value;
            var units = _units.GetEntities(_unitsBuffer);

            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];

                if (!unit.hasTargetCell || unit.isAttacking || unit.isDead || unit.isFreezeDebuff
                    || (unit.hasSurroundSlot && unit.currentCell.Value == unit.surroundSlot.Value))
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
                {
                    if (unit.isMoving)
                    {
                        unit.isMoving = false;

                        if (unit.hasVelocity)
                            unit.RemoveVelocity();
                    }

                    continue;
                }

                var movementOffset = unit.movementOffset.Value;
                var targetWorldPos = tilemap[targetCell] + movementOffset;
                var currentPos = unit.woldPos.Value;
                var dirVec = (targetWorldPos - currentPos);
                var dist = dirVec.magnitude;

                if (dist > _distThreshold)
                    dirVec /= dist;

                var speed = unit.movementSpeed.Value * unit.movementSpeedBonus.Value * _timeService.DeltaTime;
                var arrived = dist <= speed || dist <= _distThreshold;
                var newPos = arrived ? targetWorldPos : currentPos + dirVec * speed;

                unit.ReplaceVelocity(dirVec * unit.movementSpeed.Value);
                unit.ReplaceWoldPos(newPos);
                unit.transform.Value.position = newPos;
                unit.isMoving = true;

                if (arrived)
                {
                    unit.ReplaceCurrentCell(targetCell);
                    unit.ReplaceLastDirection(targetCell - currentCell);
                }
            }
        }
    }
}