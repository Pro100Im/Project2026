using Entitas;
using UnityEngine;

namespace Code.Game.Features.Attack
{
    [Game] public class AttackCooldown : IComponent { public float Value; }
    [Game] public class AttackDuration : IComponent { public float Value; }
    [Game] public class Range : IComponent { public float Value; }
    [Game] public class DetectionRange : IComponent { public float Value; }
    [Game] public class RangeAttack : IComponent { }
    [Game] public class MeleeAttack : IComponent { }
    [Game] public class Attacking : IComponent { }
    [Game] public class AttackAnimStarted : IComponent { }
    [Game] public class AttackHitPending : IComponent { }
    [Game] public class AttackAvailable : IComponent { }
    [Game] public class Hitted : IComponent { }
    [Game] public class AttackDirectionComponent : IComponent { public AttackDirection Value; }
    [Game] public class AttackerPoint : IComponent { public Vector2 Value; }
    [Game] public class FirePointOffset : IComponent { public Vector3 Value; }
    [Game] public class RangeHit : IComponent { }
    [Game] public class AreaAttack : IComponent { public float Value; }

    public enum AttackDirection
    {
        Up,
        Down,
        Side
    }
}