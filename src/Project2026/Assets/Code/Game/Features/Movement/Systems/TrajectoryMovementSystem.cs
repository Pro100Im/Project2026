using Code.Game.Common.Time;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Movement.Systems
{
    public class TrajectoryMovementSystem : IExecuteSystem
    {
        private readonly ITimeService _timeService;

        private readonly IGroup<GameEntity> _entities;

        private readonly List<GameEntity> _entitiesBuffer = new(86);

        public TrajectoryMovementSystem(GameContext context, ITimeService timeService)
        {
            _timeService = timeService;

            _entities = context.GetGroup(GameMatcher.AllOf(
                GameMatcher.Id,
                GameMatcher.OwnerId,
                GameMatcher.MovementSpeed,
                GameMatcher.View,
                GameMatcher.AttackerPoint,
                GameMatcher.TargetPoint,
                GameMatcher.TrajectoryMovement,
                GameMatcher.TrajectoryCurrentArcHeight,
                GameMatcher.TrajectoryPathProgress,
                GameMatcher.Trajectory,
                GameMatcher.Transform,
                GameMatcher.TotalDistance,
                GameMatcher.MovementAvailable));
        }

        public void Execute()
        {
            var entities = _entities.GetEntities(_entitiesBuffer);

            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                var start = entity.attackerPoint.Value;
                var end = entity.targetPoint.Value;
                var trajectory = entity.trajectory.Value;
                var arcHeight = entity.trajectoryCurrentArcHeight.Value;
                var totalDist = entity.totalDistance.Value;

                if (totalDist <= 0.001f)
                    continue;

                var currentProgress = entity.trajectoryPathProgress.Value;
                var step = (entity.movementSpeed.Value * _timeService.DeltaTime) / totalDist;
                var newProgress = Mathf.Clamp01(currentProgress + step);

                entity.ReplaceTrajectoryPathProgress(newProgress);

                var t = newProgress;
                var currentPos = Vector3.Lerp(start, end, t);
                var heightOffset = trajectory.Evaluate(t) * arcHeight;

                currentPos.y += heightOffset;

                var nextPos = CalculatePosition(start, end, t + 0.01f, trajectory, arcHeight);
                var direction = nextPos - currentPos;

                if (direction != Vector3.zero)
                {
                    var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                    entity.transform.Value.rotation = Quaternion.Euler(0, 0, angle);
                }

                entity.ReplaceWoldPos(currentPos);
                entity.transform.Value.position = currentPos;
            }
        }

        private Vector3 CalculatePosition(Vector3 start, Vector3 end, float t, AnimationCurve trajectory, float arcHeight)
        {
            t = Mathf.Clamp01(t);

            var pos = Vector3.Lerp(start, end, t);

            pos.y += trajectory.Evaluate(t) * arcHeight;

            return pos;
        }
    }
}