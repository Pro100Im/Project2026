using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Attack.Systems
{
    public class AttackActionCompleteSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _attackers;
        private readonly IGroup<GameEntity> _pendingHits;

        private readonly List<GameEntity> _attackersBuffer = new(86);
        private readonly List<GameEntity> _pendingBuffer = new(86);

        public AttackActionCompleteSystem(GameContext gameContext)
        {
            _attackers = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Attacking,
                    GameMatcher.Animator,
                    GameMatcher.Id));

            _pendingHits = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.OwnerId,
                    GameMatcher.AttackHitPending));
        }

        public void Execute()
        {
            var attackers = _attackers.GetEntities(_attackersBuffer);

            for (var i = 0; i < attackers.Count; i++)
            {
                var attacker = attackers[i];

                if (attacker.isDead)
                {
                    ClearAttackAction(attacker);
                    CancelPendingHits(attacker.id.Value);
                    continue;
                }

                if (HasPendingHit(attacker.id.Value))
                    continue;

                var stateInfo = attacker.animator.Value.GetCurrentAnimatorStateInfo(0);
                var isAttackAnim = stateInfo.IsName("AttackRight")
                    || stateInfo.IsName("AttackUp")
                    || stateInfo.IsName("AttackDown");

                if (attacker.isAttackAnimStarted && isAttackAnim && stateInfo.normalizedTime < 1f)
                    continue;

                ClearAttackAction(attacker);
            }
        }

        private static void ClearAttackAction(GameEntity attacker)
        {
            attacker.isAttacking = false;
            attacker.isAttackAnimStarted = false;
        }

        private bool HasPendingHit(int ownerId)
        {
            var pending = _pendingHits.GetEntities(_pendingBuffer);

            for (var i = 0; i < pending.Count; i++)
            {
                if (pending[i].ownerId.Value == ownerId)
                    return true;
            }

            return false;
        }

        private void CancelPendingHits(int ownerId)
        {
            var pending = _pendingHits.GetEntities(_pendingBuffer);

            for (var i = 0; i < pending.Count; i++)
            {
                var attack = pending[i];

                if (attack.ownerId.Value != ownerId)
                    continue;

                attack.isAttackHitPending = false;
                attack.isDestructed = true;

                var owner = GetGameEntityById.Get(ownerId);

                if (owner != null)
                    owner.isAttackAvailable = true;
            }
        }
    }
}
