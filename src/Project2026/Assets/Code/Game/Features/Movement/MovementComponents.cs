using Entitas;
using UnityEngine;

namespace Code.Game.Features.Movement
{
    public struct MoveCommitData
    {
        public Vector3Int Direction;
        public int Steps;
        public float StartCost;
    }

    [Game] public class MovementSpeed : IComponent { public float Value; }
    [Game] public class MovementSpeedBonus : IComponent { public float Value; }
    [Game] public class MovementAvailable : IComponent { }
    [Game] public class GridMovement : IComponent { }
    [Game] public class TrajectoryMovement : IComponent { }
    [Game] public class Moving : IComponent { }
    [Game] public class CurrentCell : IComponent { public Vector3Int Value; }
    [Game] public class MovementOffset : IComponent { public Vector3 Value; }
    [Game] public class LastDirection : IComponent { public Vector3Int Value; }
    [Game] public class MoveWait : IComponent { public float Value; }
    [Game] public class MoveCommit : IComponent { public MoveCommitData Value; }
    [Game] public class Velocity : IComponent { public Vector3 Value; }
    [Game] public class Trajectory : IComponent { public AnimationCurve Value; }
    [Game] public class TrajectoryBaseArcHeight : IComponent { public float Value; }
    [Game] public class TrajectoryCurrentArcHeight : IComponent { public float Value; }
    [Game] public class TrajectoryPathProgress : IComponent { public float Value; }
    [Game] public class TotalDistance: IComponent { public float Value; }
}