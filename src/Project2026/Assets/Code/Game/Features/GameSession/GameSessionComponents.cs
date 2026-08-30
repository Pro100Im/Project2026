using Code.Meta.Features.Game;
using Entitas;

namespace Code.Game.Features.GameSession
{
    [Game] public class GameSessionComponent : IComponent { }
    [Meta] public class EndGameMenuComponent : IComponent { public EndGameMenu Value; }
}