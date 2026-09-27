using Code.Game.Features.Town;
using System;
using UnityEngine;

namespace Code.Game.StaticData.Configs
{
    [CreateAssetMenu(menuName = "Town/TownBuildingConfig")]
    public class TownBuildingConfig : ScriptableObject
    {
        [field: SerializeField] public BuildingType Type { get; private set; }
        [field: SerializeField] public string Title { get; private set; }
        [field: SerializeField] public TownBuildingLevelData[] Levels { get; private set; }
    }

    [Serializable]
    public class TownBuildingLevelData
    {
        [field: SerializeField] public string Title { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
    }
}
