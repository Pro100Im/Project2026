using UnityEngine;

namespace Code.Game.StaticData.Configs
{
    [CreateAssetMenu(menuName = "FloatingText/FloatingTextConfig")]
    public class FloatingTextConfig : ScriptableObject
    {
        [field: SerializeField] public EntityConfig Text { get; private set; }
        [field: SerializeField] public Vector3 SpawnOffset { get; private set; }
        [field: SerializeField] public float SpawnOffsetRangeX { get; private set; }
        [field: SerializeField] public Color DamageColor { get; private set; } = new(1f, 0.35f, 0.25f, 1f);
        [field: SerializeField] public Color HealColor { get; private set; } = new(0.35f, 1f, 0.45f, 1f);
    }
}
