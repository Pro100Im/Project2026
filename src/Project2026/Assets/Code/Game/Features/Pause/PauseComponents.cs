using Code.Meta.Features.Game;
using Entitas;

namespace Assets.Code.Game.Features.Pause
{
    [Game, Meta] public class Pause : IComponent { }
    [Meta] public class PauseMenuComponent : IComponent { public PauseMenu Value; }
    [Input] public class PauseRequested : IComponent { }
    [Input] public class ForcedPauseRequested : IComponent { }
}