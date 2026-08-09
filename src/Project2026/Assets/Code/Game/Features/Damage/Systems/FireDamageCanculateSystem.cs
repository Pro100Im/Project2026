using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Damage.Systems
{
    public class FireDamageCanculateSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _damages;

        private readonly List<GameEntity> _damagesBuffer = new(86);

        public FireDamageCanculateSystem()
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
                var attacker = GetGameEntityById.Get(damage.ownerId.Value);

                if (!attacker.hasFireDamage)
                    continue;

                var target = GetGameEntityById.Get(damage.targetId.Value);
                var resistance = target.hasFireResistance ? target.fireResistance.Value : 0;
                var resistPercent = resistance / 100f;

                resistPercent = Mathf.Clamp(resistPercent, 0f, 1f);

                var fireDamage = attacker.fireDamage.Value * (1 - resistPercent);
                var totalDamage = damage.totalDamage.Value + fireDamage;

                damage.ReplaceTotalDamage(totalDamage);
            }
        }
    }
}