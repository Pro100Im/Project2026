using Entitas;

namespace Code.Game.Features.Damage
{
    [Game] public class DamageRequest : IComponent { }
    [Game] public class Damage : IComponent { public float Value; }
}