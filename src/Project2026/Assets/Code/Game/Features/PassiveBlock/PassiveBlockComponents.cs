using Entitas;

namespace Code.Game.Features.PassiveBlock
{
    [Game] public class PassiveBlockChance : IComponent { public float Value; }
    [Game] public class PassiveBlockDuration : IComponent { public float Value; }
    [Game] public class Blocking : IComponent { }
    [Game] public class BlockTimer : IComponent { }
    [Game] public class BlockRequest : IComponent { }
}
