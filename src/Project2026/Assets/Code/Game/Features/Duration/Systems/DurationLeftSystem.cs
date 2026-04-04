using Code.Common.Time;
using Entitas;

namespace Code.Game.Features.Duration.Systems
{
    public class DurationLeftSystem : IExecuteSystem
    {
        private readonly ITimeService _timeService;
        private readonly IGroup<GameEntity> _durations;

        public DurationLeftSystem(GameContext gameContext, ITimeService timeService)
        {
            _timeService = timeService;

            _durations = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Duration));
        }

        public void Execute()
        {
            foreach (var entity in _durations)
            {
                if (entity.duration.Value > 0)
                {
                    entity.ReplaceDuration(entity.duration.Value - _timeService.DeltaTime);
                }
            }
        }
    }
}