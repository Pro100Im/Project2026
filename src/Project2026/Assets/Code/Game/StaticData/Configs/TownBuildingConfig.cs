using Code.Game.Features.Town;
using System;
using UnityEngine;

namespace Code.Game.StaticData.Configs
{
    public enum TownResource
    {
        None,
        Gold,
        Stone,
        Crystal,
        Wood,
        Knowledge
    }

    [CreateAssetMenu(menuName = "Town/TownBuildingConfig")]
    public class TownBuildingConfig : ScriptableObject
    {
        [field: SerializeField] public BuildingType Type { get; private set; }
        [field: SerializeField] public string Title { get; private set; }
        [field: SerializeField] public TownResource Resource { get; private set; }
        [field: SerializeField] public TownBuildingLevelData[] Levels { get; private set; }

        public bool TryGetProduction(int level, out float interval, out int amount)
        {
            interval = 0f;
            amount = 0;

            var index = level - 1;

            if (index < 0 || Levels == null || index >= Levels.Length)
                return false;

            var data = Levels[index];
            interval = data.ProductionInterval;
            amount = data.ProductionAmount;

            return interval > 0f && amount > 0;
        }
    }

    [Serializable]
    public class TownBuildingLevelData
    {
        [field: SerializeField] public string Title { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public float ProductionInterval { get; private set; }
        [field: SerializeField] public int ProductionAmount { get; private set; }
    }
}
