using Code.Game.StaticData.Configs;
using Entitas;

namespace Code.Game.Features.Damage
{
    [Game] public class DamageRequest : IComponent { }
    [Game] public class TotalDamage : IComponent { public float Value; }

    [Game] public class PhysicalDamage : IComponent { public float Value; }
    [Game] public class FireDamage : IComponent { public float Value; }
    [Game] public class FrostDamage : IComponent { public float Value; }
    [Game] public class LightningDamage : IComponent { public float Value; }
    [Game] public class ChaosDamage : IComponent { public float Value; }

    [Game] public class PhysicalDamageHitEffect : IComponent { public EntityConfig Value; }
    [Game] public class FireDamageHitEffect : IComponent { public EntityConfig Value; }
    [Game] public class FrostDamagekHitEffect : IComponent { public EntityConfig Value; }
    [Game] public class LightningDamageHitEffect : IComponent { public EntityConfig Value; }
    [Game] public class ChaosDamagekHitEffect : IComponent { public EntityConfig Value; }
}