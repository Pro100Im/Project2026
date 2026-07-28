using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Effect.Systems
{
    public class ApplyFreezeEffectSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _effectRequests;

        private readonly List<GameEntity> _effectRequestsBuffer = new(64);

        public ApplyFreezeEffectSystem(GameContext gameContext)
        {
            _effectRequests = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.OwnerId,
                    GameMatcher.TargetId,
                    GameMatcher.TargetPoint,
                    GameMatcher.EffectCheckRequest));
        }

        public void Execute()
        {
            var requests = _effectRequests.GetEntities(_effectRequestsBuffer);

            for (var i = 0; i < requests.Count; i++)
            {
                var request = requests[i];
                var source = GetGameEntityById.Get(request.ownerId.Value);
                var target = GetGameEntityById.Get(request.targetId.Value);

                if (source == null || target == null || !source.hasFreezeEffect || target.isDead)
                    continue;

                var effect = CreateGameEntity.Empty();

                effect.AddOwnerId(source.id.Value);
                effect.AddTargetId(target.id.Value);
                effect.AddSpawnPosition(request.targetPoint.Value);
                effect.isEffect = true;
                effect.isAttached = true;

                foreach (var property in source.freezeEffect.Value.Properties)
                    property.Apply(effect);

                effect.AddDuration(effect.freezeDuration.Value);
                target.isFreezeDebuff = true;
                request.isDestructed = true;
            }
        }
    }
}
