using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Damage.Systems
{
    public class FireDamageHitEffectSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _damages;

        private readonly List<GameEntity> _damagesBuffer = new(86);

        public FireDamageHitEffectSystem()
        {
            _damages = Contexts.sharedInstance.game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.OwnerId,
                    GameMatcher.TargetId,
                    GameMatcher.TargetPoint,
                    GameMatcher.DamageRequest,
                    GameMatcher.DamageEffectRequest));
        }

        public void Execute()
        {
            var damages = _damages.GetEntities(_damagesBuffer);

            for (var i = 0; i < damages.Count; i++)
            {
                var damage = damages[i];
                var attacker = GetGameEntityById.Get(damage.ownerId.Value);

                if (!attacker.hasFireDamageHitEffect)
                    continue;

                var hitEffect = CreateGameEntity.Empty();
                hitEffect.AddSpawnPosition(damage.targetPoint.Value);

                foreach (var property in attacker.fireDamageHitEffect.Value.Properties)
                    property.Apply(hitEffect);
            }
        }
    }
}