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

            _units = context.GetGroup(GameMatcher.AllOf(
                GameMatcher.Transform,
                GameMatcher.MovementSpeed,
                GameMatcher.MovementOffset,
                GameMatcher.CurrentCell));

            _maps = context.GetGroup(GameMatcher.AllOf(
                GameMatcher.FlowField,
                GameMatcher.IntegrationField,
                GameMatcher.TilemapMovement));
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();
            var flow = map.flowField.Value;
            var integration = map.integrationField.Value;
            var tilemap = map.tilemapMovement.Value;
            var occupied = map.occupField.Value;
            var units = _units.GetEntities();

            foreach (var unit in units)
            {
                if (!unit.hasTargetCell)
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
                var targetPos = tilemap[targetCell] + movementOffset;
                var currentPos = unit.transform.Value.position;
                var dirVec = (targetPos - currentPos);
                var dist = dirVec.magnitude;

                if (dist > 0f)
                    dirVec /= dist;

                var speed = unit.movementSpeed.Value * _timeService.DeltaTime;
                var newPos = currentPos + dirVec * speed;

                unit.transform.Value.position = newPos;
                unit.isMoving = true;

                if (Vector3.Distance(newPos, targetPos) < ArriveThreshold)
                {
                    unit.ReplaceCurrentCell(targetCell);
                    unit.ReplaceLastDirection(targetCell - currentCell);
                }
            }
        }
    }
}