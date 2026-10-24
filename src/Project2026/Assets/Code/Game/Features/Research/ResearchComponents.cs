using Code.Game.Features.Town;
using Code.Game.StaticData.Configs;
using Entitas;
using UnityEngine;

namespace Code.Game.Features.Research
{
    [Game, Town] public class ResearchNode : IComponent { }
    [Game, Town] public class ResearchBonus : IComponent { }
    [Game, Town] public class ResearchIdComponent : IComponent { public ResearchId Value; }
    [Game, Town] public class ResearchLevel : IComponent { public int Value; }
    [Game, Town] public class ResearchMaxLevel : IComponent { public int Value; }
    [Game, Town] public class ResearchStartingLevel : IComponent { public int Value; }
    [Game, Town] public class ResearchCost : IComponent { public int Value; }
    [Game, Town] public class ResearchParent : IComponent { public ResearchId Value; }
    [Town] public class ResearchDirection : IComponent { public ResearchLinkDirection Value; }
    [Game, Town] public class ResearchRequiredBuilding : IComponent { public BuildingType Value; }
    [Game, Town] public class ResearchRequiredBuildingLevel : IComponent { public int Value; }

    [Game, Town] public class ResearchFireBonus : IComponent { public float Value; }
    [Game, Town] public class ResearchResistanceBonus : IComponent { public int Value; }
    [Game, Town] public class ResearchGoldBonus : IComponent { public float Value; }
    [Game, Town] public class ResearchBlockBonus : IComponent { public float Value; }
    [Game, Town] public class ResearchKnightUnlock : IComponent
    {
        public EntityConfig Config;
        public int Priority;
    }

    [Game, Town] public class ResearchUnitSlotUnlock : IComponent { public UnitKind Value; }

    [Game, Town] public class FireDamageBonus : IComponent { public float Value; }
    [Game, Town] public class GoldIncomeBonus : IComponent { public float Value; }
    [Game, Town] public class KnightBlockBonus : IComponent { public float Value; }
    [Game, Town] public class UnlockedKnight : IComponent { public EntityConfig Value; }

    [Town] public class ResearchTitle : IComponent { public string Value; }
    [Town] public class ResearchDescription : IComponent { public string Value; }
    [Town] public class ResearchIcon : IComponent { public Sprite Value; }
    [Town] public class ResearchNodePosition : IComponent { public Vector2 Value; }
    [Town] public class ResearchNodeStateComponent : IComponent { public ResearchNodeState Value; }

    [Town] public class TownResearchMenuComponent : IComponent { public Code.Meta.Features.Town.TownResearchMenu Value; }
    [Town] public class TownResearchMenuOpened : IComponent { }

    [Input] public class TownResearchMenuToggleRequested : IComponent { }
    [Input] public class TownResearchMenuCloseRequested : IComponent { }
    [Input] public class TownResearchPurchaseRequested : IComponent { public ResearchId Value; }
}

public enum ResearchId
{
    None,
    Knights,
    VeteranKnight,
    EliteKnight,
    Block1,
    Block2,
    Block3,
    Block4,
    Block5,
    Gold1,
    Gold2,
    Gold3,
    Gold4,
    Gold5,
    Fire1,
    Fire2,
    Fire3,
    Fire4,
    Fire5,
    Defense1,
    Defense2,
    Defense3,
    Defense4,
    Defense5,
    Archers
}

public enum ResearchNodeState
{
    Locked,
    Available,
    Unaffordable,
    Owned
}

public enum ResearchLinkDirection
{
    East,
    NorthEast,
    North,
    NorthWest,
    West,
    SouthWest,
    South,
    SouthEast
}
