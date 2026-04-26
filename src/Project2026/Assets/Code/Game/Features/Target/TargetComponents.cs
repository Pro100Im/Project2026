using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Target
{
    [Game] public class Targetable : IComponent { }
    [Game] [Input] public class TargetId : IComponent { public int Value; }
    [Game] public class TargetPoint : IComponent { public Vector2 Value; }
    [Game] public class TargetFlow : IComponent { public List<Vector3Int> Value; }
    [Game] public class TargetCell : IComponent { public Vector3Int Value; }
    [Game] public class WaitTimer : IComponent { public float Value; }
    [Game] public class TargetRecheckTimer : IComponent { public float Value; }
}