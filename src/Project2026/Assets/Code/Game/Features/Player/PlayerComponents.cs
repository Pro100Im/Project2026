using Code.Game.Features.Player.Service;
using Entitas;

namespace Code.Game.Features.Player
{
    [Game] public class PlayerComponent : IComponent { }
    [Game] public class PlayerAreaNumber : IComponent { public int Value; }
    [Game] public class PlayerAnimatorComponent : IComponent { public PlayerAnimatorService Value; }
}