using Code.Game.Common.Entity;
using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Attack.Systems
{
    public class RangeAttackEndSystem : IExecuteSystem
    {
        private readonly TargetService _targetService;

        private readonly IGroup<GameEntity> _rangeAttacks;
        private readonly List<GameEntity> _attacksBuffer = new(86);

        public RangeAttackEndSystem(TargetService targetService)
        {
            _targetService = targetService;

            _rangeAttacks = Contexts.sharedInstance.game.GetGroup(GameMatcher
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

                if (attack.isAttackHitPending)
                {
                    if (attack.duration.Value > 0)
                        continue;

                    var entity = GetGameEntityById.Get(attack.ownerId.Value);

                    if (entity == null || entity.isDead || !entity.hasTargetId)
                    {
                        attack.isAttackHitPending = false;

                        if (entity != null && !entity.isDead)
                        {
                            entity.isAttacking = false;
                            entity.isAttackAnimStarted = false;
                        }
                    }
                    else
                    {
                        var target = GetGameEntityById.Get(entity.targetId.Value);

                        if (target != null && !target.isDead)
                            AttackProjectileHelper.TryFire(_targetService, entity, target);

                        attack.isAttackHitPending = false;
                    }
                }

                if (attack.isAttackHitPending || attack.cooldown.Value > 0)
                    continue;

                var owner = GetGameEntityById.Get(attack.ownerId.Value);

                if (owner != null)
                    owner.isAttackAvailable = true;

                attack.isDestructed = true;
            }
        }
    }
}
