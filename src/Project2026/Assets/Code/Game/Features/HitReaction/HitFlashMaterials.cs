using UnityEngine;

namespace Code.Game.Features.HitReaction
{
    /// <summary>
    /// Shared, quantized "flash step" variants of a sprite material. A flashing renderer swaps its sharedMaterial
    /// to the variant for the current step instead of using a MaterialPropertyBlock, so idle renderers keep the
    /// base material (and batch with everything else) and renderers flashing at the same step batch together.
    /// Variants are created once per base material on first use and live for the whole app.
    /// </summary>
    public static class HitFlashMaterials
    {
        public const int Steps = 4;
        private const int MaxBases = 4;

        private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");

        private static readonly Material[] Bases = new Material[MaxBases];
        private static readonly Material[][] Variants = new Material[MaxBases][];
        private static int _count;

        /// <summary>Material to render with for <paramref name="amount"/> in 0..1; step 0 is the base itself.</summary>
        public static Material Get(Material baseMaterial, float amount)
        {
            if (baseMaterial == null)
                return null;

            var step = Mathf.Clamp(Mathf.RoundToInt(amount * Steps), 0, Steps);

            if (step == 0)
                return baseMaterial;

            var variants = VariantsFor(baseMaterial);

            return variants != null ? variants[step] : baseMaterial;
        }

        private static Material[] VariantsFor(Material baseMaterial)
        {
            for (var i = 0; i < _count; i++)
            {
                if (ReferenceEquals(Bases[i], baseMaterial) && Variants[i][Steps] != null)
                    return Variants[i];
            }

            if (!baseMaterial.HasProperty(FlashAmountId))
                return null;

            var slot = FindSlot(baseMaterial);
            var variants = Variants[slot] ??= new Material[Steps + 1];

            Bases[slot] = baseMaterial;
            variants[0] = baseMaterial;

            for (var step = 1; step <= Steps; step++)
            {
                variants[step] = new Material(baseMaterial)
                {
                    name = $"{baseMaterial.name}_Flash{step}",
                    hideFlags = HideFlags.HideAndDontSave,
                };
                variants[step].SetFloat(FlashAmountId, (float)step / Steps);
            }

            return variants;
        }

        private static int FindSlot(Material baseMaterial)
        {
            for (var i = 0; i < _count; i++)
            {
                if (ReferenceEquals(Bases[i], baseMaterial) || Bases[i] == null)
                    return i;
            }

            if (_count < MaxBases)
                return _count++;

            Debug.LogWarning($"[HitFlashMaterials] More than {MaxBases} flashable base materials; recycling slot 0.");
            return 0;
        }
    }
}
