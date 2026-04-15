using Entitas;

namespace Code.Game.Features.Death
{ 
    [Game] public class Dead : IComponent { }
    [Game] public class Killable : IComponent { }
    [Game] public class DeathDuration : IComponent { public float Value; }
}