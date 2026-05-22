using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Effect.Systems
{
    public class ApplyCombustionEffectSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _effectRequests;

        private readonly List<GameEntity> _effectRequestsBuffer = new(124);

        public ApplyCombustionEffectSystem(GameContext gameContext)
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
            var effectRequests = _effectRequests.GetEntities(_effectRequestsBuffer);

            for (var i = 0; i < effectRequests.Count; i++)
            {
                var effectRequest = effectRequests[i];
                var effectEntity = GetGameEntityById.Get(effectRequest.ownerId.Value);
                var targetEntity = GetGameEntityById.Get(effectRequest.targetId.Value);

                if (effectEntity == null || targetEntity == null || !effectEntity.hasCombustionEffect || targetEntity.isDead)
                    continue;

                var entity = CreateGameEntity.Empty();

                entity.AddOwnerId(effectEntity.ownerId.Value);
                entity.AddTargetId(targetEntity.id.Value);
                entity.AddSpawnPosition(effectRequest.targetPoint.Value);
                entity.isEffect = true;
                entity.isAttached = true;

                foreach (var property in effectEntity.combustionEffect.Value.Properties)
                    property.Apply(entity);

                entity.AddDuration(entity.combustionDuration.Value);
                entity.AddCooldown(0);

                if (targetEntity.isCombustionDebuff)
                {
                    // to do
                }
                else
                {
                    targetEntity.isCombustionDebuff = true;
                }

                effectRequest.isDestructed = true;
            }
        }
    }
}