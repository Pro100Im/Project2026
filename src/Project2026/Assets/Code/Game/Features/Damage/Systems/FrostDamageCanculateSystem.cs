using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Damage.Systems
{
    public class FrostDamageCanculateSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _damages;

        private readonly List<GameEntity> _damagesBuffer = new(86);

        public FrostDamageCanculateSystem(GameContext gameContext)
        {
            _damages = gameContext.GetGroup(GameMatcher
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

                if (!attacker.hasFrostDamage)
                    continue;

                var target = GetGameEntityById.Get(damage.targetId.Value);
                var resistance = target.hasFrostResistance ? target.frostResistance.Value : 0;
                var resistPercent = resistance / 100f;

                resistPercent = Mathf.Clamp(resistPercent, 0f, 1f);

                var frostDamage = attacker.frostDamage.Value * (1 - resistPercent);
                var totalDamage = damage.totalDamage.Value + frostDamage;

                damage.ReplaceTotalDamage(totalDamage);
            }
        }
    }
}