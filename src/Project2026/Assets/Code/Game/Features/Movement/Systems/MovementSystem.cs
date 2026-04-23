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

        public MovementSystem(GameContext gameContext, ITimeService timeService)
        {
           _timeService = timeService;

            _units = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Transform,
                    GameMatcher.MovementSpeed,
                    GameMatcher.NextCell));

            _maps = gameContext.GetGroup(GameMatcher.TilemapMovement);
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();
            var flow = map.flowField.Value;

            var units = _units.GetEntities();

            for (int i = 0; i < units.Length; i++)
            {
                var unit = units[i];
                var cell = unit.currentCell.Value;

                if (!flow.TryGetValue(cell, out var dir))
                {
                    unit.isMoving = false;

                    continue;
                }

                var flowDir = new Vector3(dir.x, dir.y, 0).normalized;
                var separation = unit.hasSeparationForce ? unit.separationForce.Value : Vector3.zero;
                var finalDir = (flowDir + separation * 2f).normalized;

                unit.isMoving = true;
                unit.transform.Value.position += finalDir * unit.movementSpeed.Value * _timeService.DeltaTime;
            }
        }
    }
}