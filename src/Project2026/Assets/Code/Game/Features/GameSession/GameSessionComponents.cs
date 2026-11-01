using Code.Meta.Features.Game;
using Entitas;

namespace Code.Game.Features.GameSession
{
    [Game] public class GameSessionComponent : IComponent { }
    [Game] public class Victory : IComponent { }
    [Meta] public class EndGameMenuComponent : IComponent { public EndGameMenu Value; }
}