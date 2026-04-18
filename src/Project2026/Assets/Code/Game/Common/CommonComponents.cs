using Code.Game.StaticData.Configs;
using Code.Infrastructure.View;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Game.Common
{
    [Game] public class Id : IComponent { [PrimaryEntityIndex] public int Value; }
    [Game] public class SortOrder : IComponent { public int Value; } 
    [Game] public class Attached: IComponent { }
    [Game] public class OwnerId : IComponent { public int Value; }

    [Game] public class EntityConfigComponent : IComponent { public EntityConfig Value; }

    [Game] public class TransformComponent : IComponent { public Transform Value; }
    [Game] public class AnimatorComponent : IComponent { public Animator Value; }
    [Game] public class SpriteRendererComponent : IComponent { public SpriteRenderer Value; }
    [Game] public class UIDocumentComponent : IComponent { public UIDocument Value; }
    [Game] public class BoundsComponent : IComponent { public SpriteRenderer Value; }
    [Game] public class TouchZoneComponent : IComponent { public SpriteRenderer Value; }

    [Game] public class View : IComponent { public IEntityView Value; }
    [Game] public class ViewPath : IComponent { public string Value; }
    [Game] public class ViewPrefab : IComponent { public EntityBehaviour Value; }
    [Game] public class SpriteComponent : IComponent { public Sprite Value; }

    [Game, Meta] public class Destructed : IComponent { }
    [Game, Meta] public class DelayDestruct : IComponent { }
}