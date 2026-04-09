using Entitas;
using System.Numerics;

namespace Code.Game.Features.Target
{
    [Game] public class Targetable : IComponent { }
    [Game] public class TargetId : IComponent { public int Value; }
    [Game] public class TargetPoint : IComponent { public Vector2 Value; }
}