using Code.Game.Features.Town.Views;
using Entitas;

namespace Code.Game.Features.Town
{
    [Game] public class TownCastle : IComponent { }
    [Game] public class TownBuildingLevel : IComponent { public int Value; }
    [Game] public class TownBuildingMaxLevel : IComponent { public int Value; }
    [Game] public class VillageViewComponent : IComponent { public TownView Value; }
    [Game] public class TowersViewComponent : IComponent { public int Value; }
    [Game] public class GoldMineViewComponent : IComponent { public int Value; }
    [Game] public class KingthBarackViewComponent : IComponent { public int Value; }
    [Game] public class ArgerBarackViewComponent : IComponent { public int Value; }
    [Game] public class CathedralViewComponent : IComponent { public int Value; }
}