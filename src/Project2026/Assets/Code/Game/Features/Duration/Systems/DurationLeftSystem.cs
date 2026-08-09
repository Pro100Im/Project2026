using Code.Game.Common.Time;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Duration.Systems
{
    public class DurationLeftSystem : IExecuteSystem
    {
        private readonly ITimeService _timeService;

        private readonly IGroup<GameEntity> _durations;

        private readonly List<GameEntity> _durationsBuffer = new(86);

        public DurationLeftSystem(ITimeService timeService)
        {
            _timeService = timeService;

            _durations = Contexts.sharedInstance.game.GetGroup(GameMatcher.Duration);
        }

        public void Execute()
        {
            var durations = _durations.GetEntities(_durationsBuffer);

            for (var i = 0; i < durations.Count; i++)
            {
                var entity = durations[i];

                if (entity.duration.Value > 0)
                    entity.ReplaceDuration(entity.duration.Value - _timeService.DeltaTime);
            }
        }
    }
}