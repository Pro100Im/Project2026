using System;
using UnityEngine;

namespace Code.Game.StaticData.Configs
{
    /// <summary>
    /// Pixel-art cloud veil (Tiny Swords style) over a map-like content. Sizes are in content UI pixels.
    /// </summary>
    [Serializable]
    public class CloudVeilSettings
    {
        [Header("Clearings")]
        [Tooltip("Clear area around available and completed levels.")]
        public float RevealRadius = 230f;

        [Tooltip("Clear area around the next (locked) levels, small so they sit on the cloud edge.")]
        public float PreviewRadius = 140f;

        [Tooltip("Clear area along visible roads.")]
        public float RoadRadius = 90f;

        [Tooltip("Distance between clear points along a road.")]
        public float RoadStep = 70f;

        [Tooltip("Cloud frame along the content edges. 0 = no frame.")]
        public float BorderWidth = 160f;

        [Header("Pixel art")]
        [Tooltip("Map UI pixels per art pixel.")]
        [Min(1f)] public float PixelSize = 2f;

        [Tooltip("Size of a cloud puff cell. Puffs are 0.55-1 of it in radius.")]
        [Min(8f)] public float PuffSize = 56f;

        [Tooltip("How far the flat cloud shadow falls below the clouds.")]
        public float ShadowOffset = 18f;

        public Color Light = Rgba(252, 254, 239, 255);
        public Color Mid = Rgba(229, 233, 205, 255);
        public Color Dark = Rgba(219, 216, 190, 255);
        public Color ShadowColor = Rgba(36, 42, 59, 85);

        [Tooltip("How much the puffs swell and sway, fraction of the puff size. 0 = static edge.")]
        [Range(0f, 0.12f)] public float BreathAmount = 0.06f;

        [Tooltip("Swell and sway speed.")]
        public float BreathSpeed = 0.8f;

        /// <summary>Defaults for the research tree: small clearings, no frame.</summary>
        public static CloudVeilSettings ForResearch() => new()
        {
            RevealRadius = 92f,
            PreviewRadius = 92f,
            RoadRadius = 58f,
            RoadStep = 40f,
            BorderWidth = 0f,
            PuffSize = 38f,
            ShadowOffset = 12f
        };

        private static Color Rgba(byte r, byte g, byte b, byte a) => new Color32(r, g, b, a);
    }
}
