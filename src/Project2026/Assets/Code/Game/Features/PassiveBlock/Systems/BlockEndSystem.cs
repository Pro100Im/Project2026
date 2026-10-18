using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.PassiveBlock.Systems
{
    public class BlockEndSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _timers;

        private readonly List<GameEntity> _timersBuffer = new(64);

        public BlockEndSystem()
        {
            _timers = Contexts.sharedInstance.game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.BlockTimer,
                    GameMatcher.TargetId,
                    GameMatcher.Duration));
        }

        public void Execute()
        {
            var timers = _timers.GetEntities(_timersBuffer);

            for (var i = 0; i < timers.Count; i++)
            {
                var timer = timers[i];

                if (timer.duration.Value > 0f)
                    continue;

                var target = GetGameEntityById.Get(timer.targetId.Value);

                if (target != null)
                    target.isBlocking = false;

                timer.isDestructed = true;
            }
        }
    }
}
