using Entitas;

namespace Code.Game.Features.Health
{
    [Game] public class MaxHealth : IComponent { public float Value; }
    [Game] public class CurrentHealth : IComponent { public float Value; }
    [Game] public class Dead : IComponent { }
}