using Entitas;
using UnityEngine;

namespace Code.Game.Features.Attack
{
    [Game] public class AttackCooldown : IComponent { public float Value; }
    [Game] public class AttackDuration : IComponent { public float Value; }
    [Game] public class Range : IComponent { public float Value; }
    [Game] public class RangeAttack : IComponent { }
    [Game] public class MeleeAttack : IComponent { }
    [Game] public class Attacking : IComponent { }
    [Game] public class AttackAvailable : IComponent { }
    [Game] public class Hitted : IComponent { }
    [Game] public class AttackDirectionComponent : IComponent { public AttackDirection Value; }
    [Game] public class AttackerPoint : IComponent { public Vector2 Value; }
    [Game] public class FirePoint : IComponent { public Vector3 Value; }

    public enum AttackDirection
    {
        Up,
        Down,
        Side
    }
}