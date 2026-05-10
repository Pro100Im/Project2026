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

            if (!entity.hasTrajectoryBaseArcHeight)
                entity.AddTrajectoryBaseArcHeight(ArcHeight);

            if (!entity.hasTrajectoryCurrentArcHeight)
                entity.AddTrajectoryCurrentArcHeight(ArcHeight);

            if (!entity.hasTrajectory)
                entity.AddTrajectory(Trajectory);

            if (!entity.hasTrajectoryPathProgress)
                entity.AddTrajectoryPathProgress(0);
        }

        protected override void Remove(GameEntity entity)
        {
            if (entity.isTrajectoryMovement)
                entity.isTrajectoryMovement = false;

            if (entity.hasTrajectoryBaseArcHeight)
                entity.RemoveTrajectoryBaseArcHeight();

            if (entity.hasTrajectoryCurrentArcHeight)
                entity.RemoveTrajectoryCurrentArcHeight();

            if (entity.hasTrajectory)
                entity.RemoveTrajectory();

            if (entity.hasTrajectoryPathProgress)
                entity.RemoveTrajectoryPathProgress();
        }

        protected override void Replace(GameEntity entity)
        {
            if (entity.hasTrajectoryBaseArcHeight)
                entity.ReplaceTrajectoryBaseArcHeight(ArcHeight);

            if (entity.hasTrajectoryCurrentArcHeight)
                entity.ReplaceTrajectoryCurrentArcHeight(ArcHeight);

            if (entity.hasTrajectory)
                entity.ReplaceTrajectory(Trajectory);

            if (entity.hasTrajectoryPathProgress)
                entity.ReplaceTrajectoryPathProgress(0);
        }
    }
}