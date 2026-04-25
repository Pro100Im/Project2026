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

        public MovementSystem(GameContext context, ITimeService timeService)
        {
            _timeService = timeService;

            _units = context.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Transform,
                    GameMatcher.MovementSpeed,
                    GameMatcher.CurrentCell));

            _maps = context.GetGroup(GameMatcher
                .AllOf(GameMatcher.FlowField, GameMatcher.TilemapMovement));
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
                var cell = unit.currentCell.Value;

                if (!flow.TryGetValue(cell, out var dir))
                    continue;

                var flowDir = new Vector3(dir.x, dir.y, 0).normalized;

                // 👉 мягкое separation (НЕ обязательное)
                var sep = unit.hasSeparationForce ? unit.separationForce.Value : Vector3.zero;

                float sepStrength = unit.isMoving ? 0.6f : 0.1f;

                Vector3 finalDir = (flowDir + sep * sepStrength).normalized;

                var speed = unit.movementSpeed.Value * _timeService.DeltaTime;

                var newPos = unit.transform.Value.position + finalDir * speed;

                unit.transform.Value.position = newPos;

                // 👉 обновление клетки только при реальном входе
                var mapPos = tilemap[cell];

                if (Vector3.Distance(newPos, mapPos) < 0.15f)
                {
                    var nextCell = cell + dir;
                    unit.ReplaceCurrentCell(nextCell);
                }
            }
        }
    }
}