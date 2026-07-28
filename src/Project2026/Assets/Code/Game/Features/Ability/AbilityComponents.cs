using Entitas;

namespace Code.Game.Features.Ability
{
    public enum AbilityTargetType
    {
        Enemies,
        Allies,
        All
    }

    [Game] public class AbilitySelectRequest : IComponent { }
    [Game] public class AbilityTargeting : IComponent { }
    [Game] public class AbilityCastRequest : IComponent { }
    [Game] public class AbilityRadius : IComponent { public float Value; }
    [Game] public class AbilityTargetFilter : IComponent { public AbilityTargetType Value; }

    [Game] public class AbilityRangeShowed : IComponent { }
}
