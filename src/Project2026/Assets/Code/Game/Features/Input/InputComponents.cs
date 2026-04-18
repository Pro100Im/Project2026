using Entitas;
using UnityEngine;

namespace Code.Game.Input
{
  [Input] public class Input : IComponent { }
  [Input] public class AxisInput : IComponent { public Vector2 Value; }
  [Input] public class PointerInput : IComponent { public Vector2 Value; }
  [Input] public class ScreenPointerInput : IComponent { public Vector2 Value; }
  [Input] public class WorldPointerInput : IComponent { public Vector2 Value; }
  [Input] public class PointerRay : IComponent { public Ray Value; }
}