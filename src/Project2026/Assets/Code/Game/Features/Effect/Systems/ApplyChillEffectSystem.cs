using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Effect.Systems
{
    public class ApplyChillEffectSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _effectRequests;

        private readonly List<GameEntity> _effectRequestsBuffer = new(124);

        public ApplyChillEffectSystem(GameContext gameContext)
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

                if (targetEntity.isDead || !effectEntity.hasChillEffect)
                    continue;

                var entity = CreateGameEntity.Empty();

                entity.AddOwnerId(effectEntity.ownerId.Value);
                entity.AddTargetId(targetEntity.id.Value);
                entity.AddSpawnPosition(effectRequest.targetPoint.Value);
                entity.isEffect = true;
                entity.isAttached = true;

                foreach (var property in effectEntity.chillEffect.Value.Properties)
                    property.Apply(entity);

                entity.AddDuration(entity.chillDuration.Value);

                if (targetEntity.isChillDebuff)
                {
                    // to do
                }
                else
                {
                    targetEntity.isChillDebuff = true;                
                }

                effectRequest.isDestructed = true;
            }
        }
    }
}