using Entitas;
using UnityEngine;

namespace Code.Game.Features.Movement
{
    [Game] public class MovementSpeed : IComponent { public float Value; }
    [Game] public class Direction : IComponent { public Vector2 Value; }
    [Game] public class Moving : IComponent { }
    [Game] public class MovementAvailable : IComponent { }
    [Game] public class RotationAlignedAlongDirection : IComponent { }
    [Game] public class MovementPoints : IComponent { public Vector3[] Value; }
    [Game] public class MovementOffsets : IComponent { public float Value; }
    [Game] public class MovementCurrentPointIndex : IComponent { public int Value; }
}