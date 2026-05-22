using Entitas;

namespace Code.Game.Features.Debaffs
{
    [Game] public class ApplyDebuffRequest : IComponent { }

    [Game] public class ChillDebuff : IComponent { }    
    [Game] public class CombustionDebuff : IComponent { }

    [Game] public class MoveSlowingDown : IComponent { public float Value; }
    [Game] public class AttackSlowingDown : IComponent { public float Value; }
}