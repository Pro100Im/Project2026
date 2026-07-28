using Code.Game.StaticData.Configs;
using Entitas;

namespace Code.Game.Features.Effect
{
    [Game] public class EffectCheckRequest : IComponent { }
    [Game] public class Effect : IComponent { }
    [Game] public class Stackable : IComponent { }

    [Game] public class CombustionEffect : IComponent { public EntityConfig Value; }
    [Game] public class CombustionDuration : IComponent { public float Value; }
    [Game] public class CombustionCoolDown : IComponent { public float Value; }

    [Game] public class ChillEffect : IComponent { public EntityConfig Value; }
    [Game] public class ChillDuration : IComponent { public float Value; }

    [Game] public class FreezeEffect : IComponent { public EntityConfig Value; }
    [Game] public class FreezeDuration : IComponent { public float Value; }
}