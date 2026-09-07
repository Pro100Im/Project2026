using Code.Game.StaticData.Configs;
using Code.Meta.Features.Game;
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

    [Game] public class CannonAimDirectionComponent : IComponent { public CannonAimDirection Value; }
    [Game] public class CannonFlipX : IComponent { }
    [Game] public class CannonAnimator : IComponent { public UnityEngine.Animator Value; }
    [Game] public class CannonSpriteRenderer : IComponent { public SpriteRenderer Value; }

    [Meta] public class TowerMenuComponent : IComponent { public TowerMenu Value; }
    [Meta] public class TowerOpenBuildMenu : IComponent { }
    [Meta] public class TowerOpenUpgradeMenu : IComponent { }
    [Meta] public class TowerMenuCloseRequest : IComponent { }

    public enum CannonAimDirection
    {
        Down = 0,
        DownRight = 1,
        Right = 2,
        UpRight = 3,
        Up = 4
    }
}
