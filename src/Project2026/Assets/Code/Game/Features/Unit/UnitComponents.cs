using Entitas;

namespace Code.Game.Features.Unit
{
    [Game] public class Unit : IComponent { }
    [Game] public class UnitSize : IComponent { public float Value; }
    [Game] public class SettleTimer : IComponent { public float Value; }
}