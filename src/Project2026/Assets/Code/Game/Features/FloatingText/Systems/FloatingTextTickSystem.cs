using Code.Game.Common.Time;
using Code.Game.StaticData.Configs;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.FloatingText.Systems
{
    /// <summary>Ages floating texts and destroys them when their float-and-fade is over.</summary>
    public class FloatingTextTickSystem : IExecuteSystem
    {
        private readonly ITimeService _timeService;
        private readonly BattleOverlayConfig _config;
        private readonly IGroup<GameEntity> _texts;
        private readonly List<GameEntity> _buffer = new(256);

        public FloatingTextTickSystem(ITimeService timeService, BattleOverlayConfig config)
        {
            _timeService = timeService;
            _config = config;
            _texts = Contexts.sharedInstance.game.GetGroup(GameMatcher.FloatingTextAge);
        }

        public void Execute()
        {
            var deltaTime = _timeService.DeltaTime;
            var duration = _config != null ? _config.Duration : 1f;
            var texts = _texts.GetEntities(_buffer);

            for (var i = 0; i < texts.Count; i++)
            {
                var text = texts[i];

                // Mutated in place: render data, nothing reacts to it.
                text.floatingTextAge.Value += deltaTime;

                if (text.floatingTextAge.Value >= duration)
                    text.isDestructed = true;
            }
        }
    }
}
