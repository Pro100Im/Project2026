using Entitas;
using System;
using UnityEngine;

namespace Code.Game.Features.Health
{
    /// <summary>Static look of an HP bar (from config). The bar has no GameObject: HpBarRenderSystem draws it.</summary>
    [Game] public class HpBar : IComponent { public HpBarShape Value; }

    /// <summary>Animated fill of an HP bar. Mutated in place every frame by the HP bar systems (no Replace).</summary>
    [Game] public class HpBarFill : IComponent { public HpBarFillState Value; }

    [Game] public class MaxHealth : IComponent { public int Value; }
    [Game] public class CurrentHealth : IComponent { public int Value; }

    [Serializable]
    public struct HpBarShape
    {
        [Tooltip("Width of the fill, world units.")]
        public float Width;
        [Tooltip("Height of the fill, world units.")]
        public float Height;
        [Tooltip("Frame around the fill, world units.")]
        public float Border;
        [Tooltip("Seconds the back fill waits before catching up, and the time it takes to catch up.")]
        public float BackDelay;
    }

    public struct HpBarFillState
    {
        /// <summary>Front fill, 0..1.</summary>
        public float Fill;
        /// <summary>Back fill, 0..1: lags behind and catches up with the front fill.</summary>
        public float BackFill;
        /// <summary>Back fill when the last change happened.</summary>
        public float BackFrom;
        /// <summary>Seconds since the last change.</summary>
        public float Elapsed;
    }
}
