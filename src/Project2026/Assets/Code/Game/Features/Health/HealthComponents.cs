using Entitas;

namespace Code.Game.Features.Health
{
    [Game] public class HpBar : IComponent { public HpBarView Value; }
    [Game] public class MaxHealth : IComponent { public float Value; }
    [Game] public class CurrentHealth : IComponent { public float Value; }
}