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
            var tilemap = map.tilemapMovement.Value;

            var units = _units.GetEntities();

            for (int i = 0; i < units.Length; i++)
            {
                var unit = units[i];
                var tr = unit.transform.Value;

                var pos = tr.position;
                var cell = unit.currentCell.Value;

                if (!flow.TryGetValue(cell, out var dir))
                {
                    unit.isMoving = false;

                    continue;
                }

                var desiredDir = dir;

                if (desiredDir == Vector3Int.zero)
                {
                    unit.isMoving = false;

                    continue;
                }

                var moveDir = new Vector3(desiredDir.x, desiredDir.y, 0).normalized;

                unit.isMoving = true;
                tr.position += moveDir * unit.movementSpeed.Value * _timeService.DeltaTime;
            }
        }
    }
}