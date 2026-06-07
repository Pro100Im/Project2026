using Code.Game.Features.Target.Services;
using Entitas;
using UnityEngine;

namespace Code.Game.Features.Unit
{
    [Game] public class Unit : IComponent { }
    [Game] public class UnitIcon : IComponent { public Sprite Value; }
    [Game] public class UnitSize : IComponent { public Vector2Int Value; }
    [Game] public class UnitAnchorPoint : IComponent { public Vector3 Value; }

    [Meta] public class UnitRangeShowed : IComponent { }
    [Meta] public class UnitSelectedView : IComponent { }
    [Meta] public class UnitRangeViewComponent : IComponent { public UnitRangeView Value; }
}