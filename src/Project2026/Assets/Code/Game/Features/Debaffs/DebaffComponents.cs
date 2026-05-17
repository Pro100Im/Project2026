using Entitas;

namespace Code.Game.Features.Debaffs
{
    [Game] public class ApplyDebuffRequest : IComponent { }
    [Game] public class MoveSlowingDown : IComponent { public float Value; }
    [Game] public class AttackSlowingDown : IComponent { public float Value; }
    [Game] public class CombustionDuration : IComponent { public float Value; }
    [Game] public class CombustionCoolDown : IComponent { public float Value; }
    [Game] public class ChillDuration : IComponent { public float Value; }
    [Game] public class ChillCoolDown : IComponent { public float Value; }
}