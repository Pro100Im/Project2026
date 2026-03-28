using Entitas;
using UnityEngine;

namespace Code.Game.Features.Attack
{
    public class AttackComponents : MonoBehaviour
    {
        [Game] public class Attack : IComponent { public float Value; }
        [Game] public class Attack—ooldown : IComponent { public float Value; }
        [Game] public class RangeAttack : IComponent { }
        [Game] public class MeleeAttack : IComponent { }
    }
}