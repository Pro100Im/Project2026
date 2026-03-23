using Entitas;
using UnityEngine;

namespace Code.Game.Features.Movement
{
    [Game] public class Speed : IComponent { public float Value; }
    [Game] public class MaxRunSpeed : IComponent { public float Value; }
    [Game] public class MaxWalkSpeed : IComponent { public float Value; }
    [Game] public class CurrentSpeed : IComponent { public float Value; }
    [Game] public class Direction : IComponent { public Vector3 Value; }
    [Game] public class LookAtPoint : IComponent { public Vector3 Value; }
    [Game] public class Moving : IComponent { }
    [Game] public class MovementAvailable : IComponent { }
    [Game] public class RotationAlignedAlongDirection : IComponent { }
    [Game] public class RotationAlignedAlongTarget : IComponent { }
}