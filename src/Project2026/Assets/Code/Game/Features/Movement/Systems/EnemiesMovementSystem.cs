using Code.Game.Common.Time;
using Code.Game.Common.Random;
using Entitas;
using UnityEngine;

namespace Code.Game.Features.Movement.Systems
{
    public class EnemiesMovementSystem : IExecuteSystem
    {
        private readonly ITimeService _timeService;
        private readonly IRandomService _randomService;

        private readonly IGroup<GameEntity> _enemies;
        private readonly IGroup<GameEntity> _movementPoints;

        public EnemiesMovementSystem(GameContext gameContext, ITimeService timeService, IRandomService randomService)
        {
            _timeService = timeService;
            _randomService = randomService;

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
              GameMatcher.MovementPointMinDistances,
              GameMatcher.MinMovementOffsets,
              GameMatcher.MaxMovementOffsets,
              GameMatcher.GateNumber,
              GameMatcher.Enemy));
        }

        public void Execute()
        {
            foreach (var movementPoint in _movementPoints)
            {
                foreach (var enemy in _enemies)
                {
                    if (enemy.gateNumber.Value != movementPoint.gateNumber.Value)
                        continue;

                    if (enemy.movementCurrentPointIndex.Value >= movementPoint.movementPoints.Value.Length || enemy.isAttacking || enemy.isDead)
                    {
                        enemy.isMoving = false;

                        continue;
                    }

                    var index = enemy.movementCurrentPointIndex.Value;
                    var targetPoint = movementPoint.movementPoints.Value[index];

                    var minOffset = movementPoint.minMovementOffsets.Value[index];
                    var maxOffset = movementPoint.maxMovementOffsets.Value[index];

                    var offsetX = _randomService.GetLocalRandom(minOffset.x, maxOffset.x, enemy.id.Value);
                    var offsetY = _randomService.GetLocalRandom(minOffset.y, maxOffset.y, enemy.id.Value);
                    
                    targetPoint.x += offsetX;
                    targetPoint.y += offsetY;

                    if (Vector2.Distance(enemy.transform.Value.position, targetPoint) <= movementPoint.movementPointMinDistances.Value[index])
                    {
                        enemy.ReplaceMovementCurrentPointIndex(enemy.movementCurrentPointIndex.Value + 1);

                        continue;
                    }

                    enemy.isMoving = true;
                    enemy.transform.Value.position = Vector2.MoveTowards(
                        enemy.transform.Value.position,
                        targetPoint,
                        enemy.movementSpeed.Value * _timeService.DeltaTime);
                }
            }
        }
    }
}