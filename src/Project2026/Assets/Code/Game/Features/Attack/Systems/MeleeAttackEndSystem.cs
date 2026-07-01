using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Attack.Systems
{
    public class MeleeAttackEndSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _attacks;

        private readonly List<GameEntity> _attacksBuffer = new(86);

        public MeleeAttackEndSystem(GameContext gameContext)
        {
            _attacks = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.OwnerId,
                    GameMatcher.Cooldown,
                    GameMatcher.Duration,
                    GameMatcher.MeleeAttack));
        }

        public void Execute()
        {
            var attacks = _attacks.GetEntities(_attacksBuffer);

            for (var i = 0; i < attacks.Count; i++)
            {
                var attack = attacks[i];

                if (attack.duration.Value > 0)
                    continue;

                var entity = GetGameEntityById.Get(attack.ownerId.Value);

                entity.isAttacking = false;

                if (attack.cooldown.Value > 0)
                    continue;

                entity.isAttackAvailable = true;
                attack.isDestructed = true;
            }
        }
    }
}
