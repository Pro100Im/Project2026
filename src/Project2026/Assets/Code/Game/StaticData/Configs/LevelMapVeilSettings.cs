using System;
using UnityEngine;

namespace Code.Game.StaticData.Configs
{
    /// <summary>
    /// Pixel-art cloud veil over the world map (Tiny Swords style). Sizes are in map UI pixels.
    /// </summary>
    [Serializable]
    public class LevelMapVeilSettings
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

        [Tooltip("Cloud frame along the map edges.")]
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

        [Header("Drift")]
        [Tooltip("Cloud drift speed, map pixels per second.")]
        public float DriftSpeed = 12f;

        [Tooltip("Whole cloud sprites that float over the veil.")]
        public Texture2D[] DriftClouds = Array.Empty<Texture2D>();

        [Min(0)] public int DriftCloudCount = 6;

        [Tooltip("A sprite texel is PixelSize x this map pixels.")]
        [Min(0.1f)] public float DriftCloudScale = 0.5f;

        private static Color Rgba(byte r, byte g, byte b, byte a) => new Color32(r, g, b, a);
    }
}
