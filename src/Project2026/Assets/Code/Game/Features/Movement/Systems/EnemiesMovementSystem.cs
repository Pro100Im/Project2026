using Code.Common.Time;
using Entitas;
using UnityEngine;

namespace Code.Game.Features.Movement.Systems
{
    public class EnemiesMovementSystem : IExecuteSystem
    {
        private readonly ITimeService _timeService;

        private readonly IGroup<GameEntity> _enemies;
        private readonly IGroup<GameEntity> _movementPoints;

        public EnemiesMovementSystem(GameContext gameContext, ITimeService timeService)
        {
            _timeService = timeService;

            _enemies = gameContext.GetGroup(GameMatcher
              .AllOf(
              GameMatcher.MovementAvailable,
              GameMatcher.MovementSpeed,
              GameMatcher.MovementCurrentPointIndex,
              GameMatcher.GateNumber,
              GameMatcher.Enemy));

            _movementPoints = gameContext.GetGroup(GameMatcher
              .AllOf(
              GameMatcher.MovementPoints,
              GameMatcher.MovementOffsets,
              GameMatcher.GateNumber,
              GameMatcher.Enemy));
        }

        public void Execute()
        {
            foreach (var movementPoint in _movementPoints)
            {
                foreach (var enemy in _enemies)
                {
                    if (enemy.movementCurrentPointIndex.Value >= movementPoint.movementPoints.Value.Length)
                        continue;

                    if (enemy.gateNumber.Value != movementPoint.gateNumber.Value)
                        continue;

                    if (Vector3.Distance(enemy.transform.Value.position, movementPoint.movementPoints.Value[enemy.movementCurrentPointIndex.Value]) <= 0.5)
                    {
                        enemy.ReplaceMovementCurrentPointIndex(enemy.movementCurrentPointIndex.Value + 1);

                        continue;
                    }

                    var index = enemy.movementCurrentPointIndex.Value;
                    var targetPoint = movementPoint.movementPoints.Value[index];
                    var offset = movementPoint.movementOffsets.Value[index];
                    var offsetX = Random.Range(-offset.x, offset.x);
                    var offsetY = Random.Range(-offset.y, offset.y);
                    
                    targetPoint.x += offsetX;
                    targetPoint.y += offsetY;

                    enemy.transform.Value.position = Vector2.MoveTowards(
                        enemy.transform.Value.position,
                        targetPoint,
                        enemy.movementSpeed.Value * _timeService.DeltaTime);
                }
            }
        }
    }
}