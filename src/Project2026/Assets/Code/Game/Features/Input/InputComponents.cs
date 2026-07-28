using Entitas;
using UnityEngine;

namespace Code.Game.Input
{
  [Input] public class Input : IComponent { }
  [Input] public class PointerState : IComponent { }
  [Input] public class PointerOverUI : IComponent { }
  [Input] public class PointerInput : IComponent { public Vector2 Value; }
  [Input] public class ScreenPointerInput : IComponent { public Vector2 Value; }
  [Input] public class WorldPointerInput : IComponent { public Vector2 Value; }

  [Input] public class ClickInput : IComponent { }
  [Input] public class PrimaryClick : IComponent { }
  [Input] public class CancelClick : IComponent { }
  [Input] public class InteractTarget : IComponent { }

  [Input] public class AbilityCastIntent : IComponent { }
  [Input] public class EntityInteractIntent : IComponent { }
  [Input] public class CancelIntent : IComponent { }
}
