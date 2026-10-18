using Code.Game.Common.Entity;
using Code.Game.Common.Random;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.PassiveBlock.Systems
{
    public class PassiveBlockSystem : IExecuteSystem
    {
        private readonly IRandomService _random;

        private readonly IGroup<GameEntity> _damages;
        private readonly IGroup<GameEntity> _timers;

        private readonly List<GameEntity> _damagesBuffer = new(86);
        private readonly List<GameEntity> _timersBuffer = new(64);

        public PassiveBlockSystem(IRandomService random)
        {
            _random = random;

            var game = Contexts.sharedInstance.game;

            _damages = game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.TotalDamage,
                    GameMatcher.TargetId,
                    GameMatcher.DamageRequest));

            _timers = game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.BlockTimer,
                    GameMatcher.TargetId,
                    GameMatcher.Duration));
        }

        public void Execute()
        {
            var damages = _damages.GetEntities(_damagesBuffer);

            for (var i = 0; i < damages.Count; i++)
            {
                var damage = damages[i];

                if (damage.isAreaDamage || damage.isBlocked)
                    continue;

                var target = GetGameEntityById.Get(damage.targetId.Value);

                if (target == null || target.isAttacking || !target.hasPassiveBlockChance)
                    continue;

                if (_random.GetGlobalRandom(0f, 1f) >= target.passiveBlockChance.Value)
                    continue;

                damage.isBlocked = true;
                damage.ReplaceTotalDamage(0f);

                var duration = target.hasPassiveBlockDuration
                    ? target.passiveBlockDuration.Value
                    : 0f;

                var timer = FindTimer(target.id.Value);

                if (timer == null)
                {
                    timer = CreateGameEntity.Empty();
                    timer.AddTargetId(target.id.Value);
                    timer.AddDuration(duration);
                    timer.isBlockTimer = true;
                }
                else
                {
                    timer.ReplaceDuration(duration);
                }

                target.isBlocking = true;
                target.isBlockRequest = true;
            }
        }

        private GameEntity FindTimer(int targetId)
        {
            var timers = _timers.GetEntities(_timersBuffer);

            for (var i = 0; i < timers.Count; i++)
            {
                var timer = timers[i];

                if (timer.isDestructed || timer.targetId.Value != targetId)
                    continue;

                return timer;
            }

            return null;
        }
    }
}
