using Entitas;

namespace Assets.Code.Game.Features.Pause
{
    [Game, Meta] public class Pause: IComponent { }
    [Input] public class PauseRequested: IComponent { }
    [Input] public class ForcedPauseRequested: IComponent { }
}