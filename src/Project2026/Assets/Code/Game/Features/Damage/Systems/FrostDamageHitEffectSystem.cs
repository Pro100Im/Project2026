using Code.Game.Common.Entity;
using Entitas;

namespace Code.Game.Features.Damage.Systems
{
    public class FrostDamageHitEffectSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _damages;

        public FrostDamageHitEffectSystem(GameContext gameContext)
        {
            _damages = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.OwnerId,
                    GameMatcher.TargetId,
                    GameMatcher.TargetPoint,
                    GameMatcher.DamageRequest));
        }

        public void Execute()
        {
            foreach (var damage in _damages)
            {
                var attacker = GetGameEntityById.Get(damage.ownerId.Value);

                if (!attacker.hasFrostDamageHitEffect)
                    continue;

                var hitEffect = CreateGameEntity.Empty();
                var targetPoint = damage.targetPoint.Value;

                if (attacker.hasMovementOffset)
                {
                    var movementOffset = attacker.movementOffset.Value;

                    targetPoint.x += movementOffset.x;
                    targetPoint.y += movementOffset.y;
                }

                hitEffect.AddSpawnPosition(targetPoint);

                foreach (var property in attacker.frostDamageHitEffect.Value.Properties)
                    property.Apply(hitEffect);
            }
        }
    }
}