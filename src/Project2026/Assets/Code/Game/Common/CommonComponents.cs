using Code.Game.StaticData.Configs;
using Code.Infrastructure.View;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Game.Common
{
    [Game, Town] public class Id : IComponent { [PrimaryEntityIndex] public int Value; }
    [Game] public class SortOrder : IComponent { public int Value; } 
    [Game, Town] public class Attached : IComponent { }
    [Game, Town] public class OwnerId : IComponent { public int Value; }
    [Game] public class TeamComponent : IComponent { public Team Value; }
    [Game] public class WoldPosComponent : IComponent { public Vector3 Value; }

    [Game, Town] public class EntityConfigComponent : IComponent { public EntityConfig Value; }

    [Game] public class TransformComponent : IComponent { public Transform Value; }
    [Game] public class AnimatorComponent : IComponent { public Animator Value; }
    [Game, Town] public class SpriteRendererComponent : IComponent { public SpriteRenderer Value; }
    [Game, Town] public class UIDocumentComponent : IComponent { public UIDocument Value; }
    [Game] public class BoundsComponent : IComponent { public SpriteRenderer Value; }
    [Game] public class TouchZoneComponent : IComponent { public SpriteRenderer Value; }
    [Game] public class Interactable : IComponent { }
    [Game] public class LineRendererComponent : IComponent { public LineRenderer Value; }

    [Game, Town] public class View : IComponent { public IEntityView Value; }
    [Game] public class ViewPath : IComponent { public string Value; }
    [Game] public class ViewPrefab : IComponent { public EntityBehaviour Value; }
    [Game, Town] public class SpriteComponent : IComponent { public Sprite Value; }

    [Game, Input, Meta, Town] public class Destructed : IComponent { }
    [Game, Meta, Town] public class DelayDestruct : IComponent { }
}

public enum Team
{
    None,
    Player,
    Enemy
}