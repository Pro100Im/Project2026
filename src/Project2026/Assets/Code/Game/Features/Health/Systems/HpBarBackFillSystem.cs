using Code.Game.Common.Time;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Health.Systems
{
    /// <summary>
    /// The back fill waits <see cref="HpBarShape.BackDelay"/> after a change, then catches up with the front fill
    /// over the same time with an InSine ease (same feel as the old per-bar tween, without a tween per bar).
    /// </summary>
    public class HpBarBackFillSystem : IExecuteSystem
    {
        private readonly ITimeService _timeService;
        private readonly IGroup<GameEntity> _hpBars;

        private readonly List<GameEntity> _buffer = new(512);

        public HpBarBackFillSystem(ITimeService timeService)
        {
            _timeService = timeService;
            _hpBars = Contexts.sharedInstance.game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.HpBar,
                    GameMatcher.HpBarFill));
        }

        public void Execute()
        {
            var deltaTime = _timeService.DeltaTime;
            var hpBars = _hpBars.GetEntities(_buffer);

            for (var i = 0; i < hpBars.Count; i++)
            {
                var entity = hpBars[i];
                var fill = entity.hpBarFill;

                // Settled: nothing to animate.
                if (fill.Value.BackFill == fill.Value.Fill)
                    continue;

                var delay = Mathf.Max(0.0001f, entity.hpBar.Value.BackDelay);

                fill.Value.Elapsed += deltaTime;

                var t = (fill.Value.Elapsed - delay) / delay;

                if (t <= 0f)
                    continue;

                if (t >= 1f)
                {
                    fill.Value.BackFill = fill.Value.Fill;
                    continue;
                }

                var eased = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
                fill.Value.BackFill = Mathf.LerpUnclamped(fill.Value.BackFrom, fill.Value.Fill, eased);
            }
        }
    }
}
