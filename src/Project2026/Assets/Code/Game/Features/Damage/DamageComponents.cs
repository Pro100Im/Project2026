using Code.Game.StaticData.Configs;
using Entitas;

namespace Code.Game.Features.Damage
{
    [Game] public class DamageRequest : IComponent { }
    [Game] public class DamageEffectRequest : IComponent { }
    [Game] public class AreaDamage : IComponent { }
    [Game] public class Blocked : IComponent { }
    [Game] public class TotalDamage : IComponent { public int Value; }

    [Game] public class PhysicalDamage : IComponent { public int Value; }
    [Game] public class FireDamage : IComponent { public int Value; }
    [Game] public class FrostDamage : IComponent { public int Value; }
    [Game] public class LightningDamage : IComponent { public int Value; }
    [Game] public class ChaosDamage : IComponent { public int Value; }

    [Game] public class PhysicalDamageHitEffect : IComponent { public EntityConfig Value; }
    [Game] public class FireDamageHitEffect : IComponent { public EntityConfig Value; }
    [Game] public class FrostDamageHitEffect : IComponent { public EntityConfig Value; }
    [Game] public class LightningDamageHitEffect : IComponent { public EntityConfig Value; }
    [Game] public class ChaosDamageHitEffect : IComponent { public EntityConfig Value; }
}
