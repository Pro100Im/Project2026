using Code.Game.Common.Time;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Cooldown.Systems
{
    public class CooldownLeftSystem : IExecuteSystem
    {
        private readonly ITimeService _timeService;

        private readonly IGroup<GameEntity> _cooldowns;

        private readonly List<GameEntity> _cooldownsBuffer = new(86);

        public CooldownLeftSystem(ITimeService timeService)
        {
            _timeService = timeService;

            _cooldowns = Contexts.sharedInstance.game.GetGroup(GameMatcher.Cooldown);
        }

        public void Execute()
        {
            var cooldowns = _cooldowns.GetEntities(_cooldownsBuffer);

            for (var i = 0; i < cooldowns.Count; i++)
            {
                var entity = cooldowns[i];

                if (entity.cooldown.Value > 0)
                    entity.ReplaceCooldown(entity.cooldown.Value - _timeService.DeltaTime);
            }
        }
    }
}