using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Damage.Systems
{
    public class FrostDamageCalculateSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _damages;
        private readonly List<GameEntity> _damagesBuffer = new(86);

        public FrostDamageCalculateSystem()
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

                if (!attacker.hasFrostDamage)
                    continue;

                var target = GetGameEntityById.Get(damage.targetId.Value);
                var resistance = target.hasFrostResistance ? target.frostResistance.Value : 0;

                if (target.hasResistanceBonus)
                    resistance += target.resistanceBonus.Value;

                var dealt = Mathf.Max(0, attacker.frostDamage.Value - resistance);
                damage.ReplaceTotalDamage(damage.totalDamage.Value + dealt);
            }
        }
    }
}
