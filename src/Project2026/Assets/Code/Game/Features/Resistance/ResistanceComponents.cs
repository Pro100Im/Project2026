using Entitas;

namespace Code.Game.Features.Resistance
{ 
    [Game] public class PhysicalResistance : IComponent { public int Value; }
    [Game] public class FrostResistance : IComponent { public int Value; }
    [Game] public class FireResistance : IComponent { public int Value; }
}