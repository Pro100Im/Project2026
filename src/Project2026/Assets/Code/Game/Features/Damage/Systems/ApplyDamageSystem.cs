using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Damage.Systems
{
    public class ApplyDamageSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _damages;

        private readonly List<GameEntity> _damagesBuffer = new(86);

        public ApplyDamageSystem()
        {
            _damages = Contexts.sharedInstance.game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.TotalDamage,
                    GameMatcher.OwnerId,
                    GameMatcher.TargetId,
                    GameMatcher.DamageRequest));
        }

        public void Execute()
        {
            var damages = _damages.GetEntities(_damagesBuffer);

            for (var i = 0; i < damages.Count; i++)
            {
                var damage = damages[i];
                var targetId = damage.targetId.Value;
                var target = GetGameEntityById.Get(targetId);
                var damageAmount = damage.totalDamage.Value;

                if (target.hasCurrentHealth)
                {
                    var newCurrentHealth = target.currentHealth.Value - damageAmount;

                    if (newCurrentHealth < 0)
                    {
                        newCurrentHealth = 0;
                    }

                    target.ReplaceCurrentHealth(newCurrentHealth);

                    if (damageAmount > 0f)
                        RequestHitReaction(damage, target);
                }

                damage.isDestructed = true;
            }
        }

        private static void RequestHitReaction(GameEntity damage, GameEntity target)
        {
            if (!target.hasHitReaction)
                return;

            var direction = GetHitDirection(damage, target);

            if (target.hasHitReactionRequest)
                target.ReplaceHitReactionRequest(direction);
            else
                target.AddHitReactionRequest(direction);
        }

        private static Vector2 GetHitDirection(GameEntity damage, GameEntity target)
        {
            if (!target.hasWoldPos)
                return Vector2.up;

            var owner = GetGameEntityById.Get(damage.ownerId.Value);

            if (owner == null || !owner.hasWoldPos)
                return Vector2.up;

            var delta = (Vector2)(target.woldPos.Value - owner.woldPos.Value);

            if (delta.sqrMagnitude <= 0.0001f)
                return Vector2.up;

            return delta.normalized;
        }
    }
}