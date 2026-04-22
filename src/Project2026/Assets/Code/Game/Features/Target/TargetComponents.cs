using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Target
{
    [Game] public class Targetable : IComponent { }
    [Game] public class Path : IComponent { public Queue<Vector3Int> Value; }
    [Game] [Input] public class TargetId : IComponent { public int Value; }
    [Game] public class TargetPoint : IComponent { public Vector2 Value; }
}