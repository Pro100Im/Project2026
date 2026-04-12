using Code.Game.Common.Time;
using Entitas;

namespace Code.Game.Features.Cooldown.Systems
{
    public class CooldownLeftSystem : IExecuteSystem
    {
        private readonly ITimeService _timeService;
        private readonly IGroup<GameEntity> _cooldowns;

        public CooldownLeftSystem(GameContext gameContext, ITimeService timeService)
        {
            _timeService = timeService;

            _cooldowns = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Cooldown));
        }

        public void Execute()
        {
            foreach (var entity in _cooldowns)
            {
                if (entity.cooldown.Value > 0)
                {
                    entity.ReplaceCooldown(entity.cooldown.Value - _timeService.DeltaTime);
                }
            }
        }
    }
}