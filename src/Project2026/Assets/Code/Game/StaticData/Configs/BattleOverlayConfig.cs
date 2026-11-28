using TMPro;
using UnityEngine;

namespace Code.Game.StaticData.Configs
{
    /// <summary>
    /// Look of the batched battle overlays: HP bars and damage numbers. Each overlay is one mesh, one draw call
    /// (see BatchedQuadMesh), instead of a GameObject per bar / per number.
    /// </summary>
    [CreateAssetMenu(menuName = "Rendering/BattleOverlayConfig")]
    public class BattleOverlayConfig : ScriptableObject
    {
        [Header("HP bars")]
        [field: SerializeField] public Material HpBarMaterial { get; private set; }
        [field: SerializeField] public int HpBarSortingOrder { get; private set; } = 9;
        [field: SerializeField] public Color HpBarFrontColor { get; private set; } = new(0.996f, 0.239f, 0.239f, 1f);
        [field: SerializeField] public Color HpBarBackFillColor { get; private set; } = new(1f, 0.922f, 0.804f, 1f);
        [field: SerializeField] public Color HpBarBackColor { get; private set; } = new(0.226f, 0.146f, 0.11f, 1f);

        [Header("Damage numbers")]
        [field: SerializeField] public Material DigitsMaterial { get; private set; }
        [field: SerializeField] public TMP_FontAsset Font { get; private set; }
        [field: SerializeField] public int TextSortingOrder { get; private set; } = 50;
        [Tooltip("Same meaning as TextMeshPro (3D) font size.")]
        [field: SerializeField] public float FontSize { get; private set; } = 5f;
        [field: SerializeField] public float Duration { get; private set; } = 1.4f;
        [field: SerializeField] public float FloatDistance { get; private set; } = 1f;
        [Tooltip("At most this many numbers on screen; extra ones are not drawn.")]
        [field: SerializeField] public int MaxTexts { get; private set; } = 256;
        [Tooltip("SDF padding around a glyph quad, atlas pixels (covers the outline).")]
        [field: SerializeField] public float GlyphPadding { get; private set; } = 5f;
    }
}
