using Code.Meta.UI.MainMenu;
using Entitas;
using UnityEngine;

namespace Code.Meta.Features.LevelMap
{
    [Meta] public class LevelNode : IComponent { }
    [Meta] public class LevelNodeId : IComponent { public string Value; }
    [Meta] public class LevelNodeRequires : IComponent { public string[] Value; }
    [Meta] public class LevelNodeRequireAll : IComponent { }
    [Meta] public class LevelNodePosition : IComponent { public Vector2 Value; }
    [Meta] public class LevelNodeTitle : IComponent { public string Value; }
    [Meta] public class LevelNodeIcon : IComponent { public Sprite Value; }
    [Meta] public class LevelNodeCompleted : IComponent { }
    [Meta] public class LevelNodeStateComponent : IComponent { public LevelNodeState Value; }
    [Meta] public class LevelNodeStars : IComponent { public int Value; }

    [Meta] public class LevelMapMenuComponent : IComponent { public LevelSelectMenu Value; }
    [Meta] public class LevelMapMenuOpened : IComponent { }
    [Meta] public class LevelMapDirty : IComponent { }
    [Meta] public class LevelMapSize : IComponent { public Vector2 Value; }
    [Meta] public class LevelMapBackground : IComponent { public Sprite Value; }

    [Input] public class LevelMapMenuOpenRequested : IComponent { }
    [Input] public class LevelMapMenuCloseRequested : IComponent { }
    [Input] public class LevelNodeChooseRequested : IComponent { public string Value; }
}

public enum LevelNodeState
{
    Locked,
    Available,
    Completed
}
