using Code.Game.Features.Town.Views;
using Code.Game.StaticData.Configs;
using Code.Meta.Features.Town;
using Entitas;

namespace Code.Game.Features.Town
{
    [Town] public class TownBuilding : IComponent { }
    [Town] public class TownBuildingType : IComponent { public BuildingType Value; }
    [Town] public class TownBuildingLevel : IComponent { public int Value; }
    [Town] public class TownBuildingMaxLevel : IComponent { public int Value; }
    [Town] public class TownBuildingConfigComponent : IComponent { public TownBuildingConfig Value; }
    [Town] public class TownViewComponent : IComponent { public TownView Value; }

    [Town] public class TownBuildingWorkers : IComponent { public int Value; }
    [Town] public class TownProductionCooldown : IComponent { public float Value; }
    [Town] public class TownResourceGained : IComponent { public int Value; }

    [Town] public class TownResources : IComponent { }
    [Town] public class TownGold : IComponent { public int Value; }
    [Town] public class TownStone : IComponent { public int Value; }
    [Town] public class TownCrystal : IComponent { public int Value; }
    [Town] public class TownWood : IComponent { public int Value; }
    [Town] public class TownKnowledge : IComponent { public int Value; }

    [Town] public class TownBuildMenuComponent : IComponent { public TownBuildMenu Value; }
    [Town] public class TownBuildMenuOpened : IComponent { }

    [Input] public class TownBuildMenuToggleRequested : IComponent { }
    [Input] public class TownBuildMenuCloseRequested : IComponent { }
    [Input] public class TownBuildRequested : IComponent { public BuildingType Value; }

    public enum BuildingType
    {
        Houses,
        Barracks,
        GoldMine,
        StoneMine,
        CrystalMine,
        Sawmill,
        Church,
        Archery,
        Towers
    }

    public enum TownBuildSlotState
    {
        Built,
        Available,
        Locked
    }
}
