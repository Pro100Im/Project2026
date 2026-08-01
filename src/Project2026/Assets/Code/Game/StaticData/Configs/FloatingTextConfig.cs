using UnityEngine;

namespace Code.Game.StaticData.Configs
{
    [CreateAssetMenu(menuName = "FloatingText/FloatingTextConfig")]
    public class FloatingTextConfig : ScriptableObject
    {
        [field: SerializeField] public EntityConfig Text { get; private set; }
        [field: SerializeField] public Vector3 SpawnOffset { get; private set; }
        [field: SerializeField] public float SpawnOffsetRangeX { get; private set; }
    }
}
