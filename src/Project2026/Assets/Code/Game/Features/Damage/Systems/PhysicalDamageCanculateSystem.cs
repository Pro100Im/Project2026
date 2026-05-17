using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Damage.Systems
{
    public class PhysicalDamageCanculateSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _damages;

        private readonly List<GameEntity> _damagesBuffer = new(86);

        public PhysicalDamageCanculateSystem(GameContext gameContext)
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

                if(!attacker.hasPhysicalDamage)
                    continue;

                var target = GetGameEntityById.Get(damage.targetId.Value);
                var resistance = target.hasPhysicalResistance ? target.physicalResistance.Value : 0;
                var resistPercent = resistance / 100f;

                resistPercent = Mathf.Clamp(resistPercent, 0f, 1f);

                var physicalDamage = attacker.physicalDamage.Value * (1 - resistPercent);
                var totalDamage = damage.totalDamage.Value  + physicalDamage;

                damage.ReplaceTotalDamage(totalDamage);
            }
        }
    }
}