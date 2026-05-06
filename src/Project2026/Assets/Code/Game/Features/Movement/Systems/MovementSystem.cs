using Code.Game.Common.Time;
using Entitas;
using UnityEngine;

namespace Code.Game.Features.Movement.Systems
{
    public class MovementSystem : IExecuteSystem
    {
        private readonly ITimeService _timeService;
        private readonly IGroup<GameEntity> _units;
        private readonly IGroup<GameEntity> _maps;

        private const float ArriveThreshold = 0.1f;

        public MovementSystem(GameContext context, ITimeService timeService)
        {
            _timeService = timeService;

            _units = context.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.Transform,
                GameMatcher.MovementSpeed,
                GameMatcher.MovementOffset,
                GameMatcher.CurrentCell,
                GameMatcher.UnitSize));

            _maps = context.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.FlowFields,
                GameMatcher.IntegrationFields,
                GameMatcher.TilemapMovement));
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();
            var allFlows = map.flowFields.Value;
            var tilemap = map.tilemapMovement.Value;
            var units = _units.GetEntities();

            foreach (var unit in units)
            {
                if (!unit.hasTargetCell || unit.isAttacking || unit.hasTargetId || unit.isDead)
                {
                    unit.isMoving = false;

                    continue;
                }

                var targetCell = unit.targetCell.Value;
                var currentCell = unit.currentCell.Value;

                if (targetCell == currentCell)
                {
                    unit.isMoving = false;

                    continue;
                }


                var movementOffset = unit.movementOffset.Value;
                var targetWorldPos = tilemap[targetCell] + movementOffset;
                var currentPos = unit.transform.Value.position;
                var dirVec = (targetWorldPos - currentPos);
                var dist = dirVec.magnitude;

                if (dist > 0.001f)
                    dirVec /= dist;

                var speed = unit.movementSpeed.Value * _timeService.DeltaTime;
                var newPos = currentPos + dirVec * speed;

                unit.transform.Value.position = newPos;
                unit.isMoving = true;

                if (Vector3.Distance(newPos, targetWorldPos) < ArriveThreshold)
                {
                    unit.ReplaceCurrentCell(targetCell);

                    var moveStep = targetCell - currentCell;

                    unit.ReplaceLastDirection(moveStep);
                }
            }
        }
    }
}