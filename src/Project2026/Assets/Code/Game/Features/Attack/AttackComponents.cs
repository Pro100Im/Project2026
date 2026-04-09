using Code.Infrastructure.View;
using Entitas;

namespace Code.Game.Features.Attack
{
    [Game] public class AttackHitEffect : IComponent { public EntityBehaviour Value; }
    [Game] public class AttackCooldown : IComponent { public float Value; }
    [Game] public class AttackDuration : IComponent { public float Value; }
    [Game] public class Range : IComponent { public float Value; }
    [Game] public class RangeAttack : IComponent { }
    [Game] public class MeleeAttack : IComponent { }
    [Game] public class Attacking : IComponent { }
    [Game] public class AttackAvailable : IComponent { }
    [Game] public class Hitted : IComponent { }
    [Game] public class AttackDirectionComponent : IComponent { public AttackDirection Value; }

    public enum AttackDirection
    {
        Up,
        Down,
        Side
    }
}