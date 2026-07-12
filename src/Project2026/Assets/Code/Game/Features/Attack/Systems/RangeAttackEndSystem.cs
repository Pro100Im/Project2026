using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Attack.Systems
{
    public class RangeAttackEndSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _rangeAttacks;
        private readonly List<GameEntity> _attacksBuffer = new(86);

        public RangeAttackEndSystem(GameContext gameContext)
        {
            _rangeAttacks = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.OwnerId,
                    GameMatcher.Cooldown,
                    GameMatcher.Duration,
                    GameMatcher.RangeAttack));
        }

        public void Execute()
        {
            var attacks = _rangeAttacks.GetEntities(_attacksBuffer);

            for (var i = 0; i < attacks.Count; i++)
            {
                var attack = attacks[i];

                if (attack.duration.Value > 0)
                    continue;

                var entity = GetGameEntityById.Get(attack.ownerId.Value);

                if (entity == null)
                {
                    attack.isDestructed = true;
                    continue;
                }

                entity.isAttacking = false;

                if (attack.cooldown.Value > 0)
                    continue;

                entity.isAttackAvailable = true;
                attack.isDestructed = true;
            }
        }
    }
}
