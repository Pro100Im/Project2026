using Code.Game.Common.Entity;
using Entitas;
using UnityEngine;

namespace Code.Game.Features.Damage.Systems
{
    public class PhysicalDamageHitEffectSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _damages;

        public PhysicalDamageHitEffectSystem(GameContext gameContext)
        {
            _damages = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.OwnerId,
                    GameMatcher.TargetId,
                    GameMatcher.DamageRequest));
        }

        public void Execute()
        {
            foreach (var damage in _damages)
            {
                var attacker = GetGameEntityById.Get(damage.ownerId.Value);

                if (!attacker.hasPhysicalDamageHitEffect)
                    continue;

                var hitEffect = CreateGameEntity.Empty();
                var targetPoint = attacker.targetPoint.Value;

                if (attacker.hasMovementOffset)
                {
                    var movementOffset = attacker.movementOffset.Value;

                    targetPoint.x += movementOffset.x;
                    targetPoint.y += movementOffset.y;
                }

                hitEffect.AddSpawnPosition(targetPoint);

                foreach (var property in attacker.physicalDamageHitEffect.Value.Properties)
                    property.Apply(hitEffect);
            }
        }
    }
}