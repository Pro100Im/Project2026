using Code.Game.StaticData.Configs;
using Entitas;
using UnityEngine;

namespace Code.Game.Features.Tower
{
    [Game] public class Tower : IComponent { }
    [Game] public class TowerPlace : IComponent { }
    [Game] public class TowerBuildRequest : IComponent { }
    [Game] public class TowerUpgrade : IComponent { public EntityConfig[] Value; }
    [Game] public class TowerUpgradeIcon : IComponent { public Sprite[] Value; }
    [Game] public class TowerUpgradePrice : IComponent { public int[] Value; }
    [Game] public class TowerUpgradeRequest : IComponent { public int Value; }
}