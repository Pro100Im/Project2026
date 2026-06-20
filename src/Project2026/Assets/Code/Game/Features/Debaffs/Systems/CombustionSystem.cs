using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Assets.Code.Game.Features.Debaffs.Systems
{
    public class CombustionSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _effects;

        private readonly List<GameEntity> _effectsBuffer = new(256);

        public CombustionSystem(GameContext gameContext)
        {
            _effects = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Id,
                    GameMatcher.TargetId,
                    GameMatcher.Cooldown,
                    GameMatcher.CombustionCoolDown,
                    GameMatcher.Effect));
        }

        public void Execute()
        {
            var effects = _effects.GetEntities(_effectsBuffer);

            // To do stacking of slowing down effects.
            for (var i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];

                if (effect.cooldown.Value > 0)
                    continue;

                var targetEntity = GetGameEntityById.Get(effect.targetId.Value);

                if (targetEntity != null && !targetEntity.isDead && targetEntity.isCombustionDebuff)
                {
                    var damage = CreateGameEntity.Empty();

                    damage.AddOwnerId(effect.id.Value);
                    damage.AddTargetId(effect.targetId.Value);
                    damage.AddTargetPoint(targetEntity.woldPos.Value);
                    damage.AddTotalDamage(0);
                    damage.isDamageRequest = true;
                    //damage.isDamageEffectRequest = true;

                    effect.ReplaceCooldown(effect.combustionCoolDown.Value);
                }
            }
        }
    }
}