using UnityEngine;

namespace Code.Game.StaticData.Configs
{
    [CreateAssetMenu(menuName = "Game/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        public const int MaxStars = 3;

        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public string Title { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public GameObject LevelPrefab { get; private set; }
        [field: SerializeField] public WavesConfig Waves { get; private set; }

        [field: Header("Stars (castle health left on victory)")]
        [field: SerializeField, Range(0f, 1f)] public float ThreeStarsHealth { get; private set; } = 0.9f;
        [field: SerializeField, Range(0f, 1f)] public float TwoStarsHealth { get; private set; } = 0.5f;

        public int ResolveStars(float healthRatio)
        {
            if (healthRatio >= ThreeStarsHealth)
                return 3;

            return healthRatio >= TwoStarsHealth ? 2 : 1;
        }
    }
}
