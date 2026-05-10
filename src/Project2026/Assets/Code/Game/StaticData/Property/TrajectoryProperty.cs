using System;
using UnityEngine;

namespace Code.Game.StaticData.Property
{
    [Serializable]
    public class TrajectoryProperty : EntityProperty
    {
        [field: SerializeField] public float ArcHeight { get; private set; }
        [field: SerializeField] public AnimationCurve Trajectory { get; private set; }

        protected override void Add(GameEntity entity)
        {
            if (!entity.isTrajectoryMovement)
                entity.isTrajectoryMovement = true;

            if (!entity.hasTrajectoryArcHeight)
                entity.AddTrajectoryArcHeight(ArcHeight);

            if (!entity.hasTrajectory)
                entity.AddTrajectory(Trajectory);

            if (!entity.hasTrajectoryPathProgress)
                entity.AddTrajectoryPathProgress(0);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.isTrajectoryMovement)
                entity.isTrajectoryMovement = false;

            if (entity.hasTrajectoryArcHeight)
                entity.RemoveTrajectoryArcHeight();

            if (entity.hasTrajectory)
                entity.RemoveTrajectory();

            if (entity.hasTrajectoryPathProgress)
                entity.RemoveTrajectoryPathProgress();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasTrajectoryArcHeight)
                entity.ReplaceTrajectoryArcHeight(ArcHeight);

            if (entity.hasTrajectory)
                entity.ReplaceTrajectory(Trajectory);

            if (entity.hasTrajectoryPathProgress)
                entity.ReplaceTrajectoryPathProgress(0);
        }
    }
}