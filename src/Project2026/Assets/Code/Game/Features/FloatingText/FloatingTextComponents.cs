using Entitas;
using UnityEngine;

namespace Code.Game.Features.FloatingText
{
    [Game] public class FloatingText : IComponent { public float Value; }
    [Game] public class FloatingTextColor : IComponent { public Color Value = Color.white; }
    [Game] public class DamageFloatingText : IComponent { }
    [Game] public class HealFloatingText : IComponent { }
}
