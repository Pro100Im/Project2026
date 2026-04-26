using Entitas;
using UnityEngine;

namespace Code.Game.Features.Movement
{
    [Game] public class MovementSpeed : IComponent { public float Value; }
    [Game] public class MovementAvailable : IComponent { }
    [Game] public class RotationAlignedAlongDirection : IComponent { }
    [Game] public class Moving : IComponent { }
    [Game] public class CurrentCell : IComponent { public Vector3Int Value; }
    [Game] public class MovementOffset : IComponent { public Vector3 Value; }
    [Game] public class LastDirection : IComponent { public Vector3Int Value; }
}