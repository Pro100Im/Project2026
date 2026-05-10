using Code.Game.Common.Time;
using Entitas;
using UnityEngine;

namespace Code.Game.Features.Movement.Systems
{
    public class TrajectoryMovementSystem : IExecuteSystem
    {
        private readonly ITimeService _timeService;

        private readonly IGroup<GameEntity> _entities;

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
                GameMatcher.TrajectoryArcHeight,
                GameMatcher.TrajectoryPathProgress,
                GameMatcher.Trajectory,
                GameMatcher.Transform,
                GameMatcher.MovementAvailable));
        }

        public void Execute()
        {
            foreach (var entity in _entities)
            {
                var start = entity.attackerPoint.Value;
                var end = entity.targetPoint.Value;
                var trajectory = entity.trajectory.Value;
                var arcHeight = entity.trajectoryArcHeight.Value;
                var totalDist = Vector3.Distance(start, end);

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