using Entitas;

namespace Code.Game.Features.Attack
{
    [Game] public class Attack : IComponent { public float Value; }
    [Game] public class Range : IComponent { public float Value; }
    [Game] public class RangeAttack : IComponent { }
    [Game] public class MeleeAttack : IComponent { }
    [Game] public class Attacking : IComponent { }
    [Game] public class Hitted : IComponent { }
}