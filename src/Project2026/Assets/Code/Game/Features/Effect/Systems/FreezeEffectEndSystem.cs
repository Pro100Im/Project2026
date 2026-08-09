using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Effect.Systems
{
    public class FreezeEffectEndSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _effects;

        private readonly List<GameEntity> _effectsBuffer = new(64);

        public FreezeEffectEndSystem()
        {
            _effects = Contexts.sharedInstance.game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.TargetId,
                    GameMatcher.Duration,
                    GameMatcher.FreezeDuration,
                    GameMatcher.Effect));
        }

        public void Execute()
        {
            var effects = _effects.GetEntities(_effectsBuffer);

            for (var i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];

                if (effect.duration.Value > 0)
                    continue;

                var target = GetGameEntityById.Get(effect.targetId.Value);

                if (target != null && !HasActiveFreeze(effects, effect))
                    target.isFreezeDebuff = false;

                effect.isDestructed = true;
            }
        }

        private bool HasActiveFreeze(List<GameEntity> effects, GameEntity endingEffect)
        {
            for (var i = 0; i < effects.Count; i++)
            {
                var other = effects[i];

                if (other == endingEffect
                    || other.targetId.Value != endingEffect.targetId.Value
                    || other.duration.Value <= 0)
                    continue;

                return true;
            }

            return false;
        }
    }
}
