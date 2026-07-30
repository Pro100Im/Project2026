using Entitas;

namespace Code.Game.Features.FloatingText
{
    [Game] public class FloatingText : IComponent { public float Value; }
    [Game] public class DamageFloatingText : IComponent { }
    [Game] public class HealFloatingText : IComponent { }
}
