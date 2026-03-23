using Code.Game.Features.Player.Service;
using Entitas;
using System.Numerics;

namespace Code.Game.Features.Player
{
    [Game] public class PlayerComponent : IComponent { }
    [Game] public class PlayerAnimatorComponent : IComponent { public PlayerAnimatorService Value; }
}