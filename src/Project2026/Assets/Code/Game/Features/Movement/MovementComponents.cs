using Entitas;
using UnityEngine;

namespace Code.Game.Features.Movement
{
    [Game] public class MovementSpeed : IComponent { public float Value; }
    [Game] public class MovementAvailable : IComponent { }
    [Game] public class RotationAlignedAlongDirection : IComponent { }
    [Game] public class Moving : IComponent { }
    [Game] public class MovementOffset : IComponent { public Vector3 Value; }
    [Game] public class MovementPoints : IComponent { public Vector2[] Value; }
    [Game] public class MinMovementOffsets : IComponent { public Vector2[] Value; }
    [Game] public class MaxMovementOffsets : IComponent { public Vector2[] Value; }
    [Game] public class MovementPointMinDistances : IComponent { public float[] Value; }
    [Game] public class MovementCurrentPointIndex : IComponent { public int Value; }
}