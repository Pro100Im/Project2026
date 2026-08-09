using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Attack.Systems
{
    public class MeleeAttackEndSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _attacks;

        private readonly List<GameEntity> _attacksBuffer = new(86);

        public MeleeAttackEndSystem()
        {
            _attacks = Contexts.sharedInstance.game.GetGroup(GameMatcher
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
                        var damage = CreateGameEntity.Empty();

                        damage.AddOwnerId(entity.id.Value);
                        damage.AddTargetId(entity.targetId.Value);
                        damage.AddTargetPoint(entity.targetPoint.Value);
                        damage.AddTotalDamage(0);
                        damage.isDamageRequest = true;
                        damage.isDamageEffectRequest = true;

                        var damageEffect = CreateGameEntity.Empty();

                        damageEffect.AddOwnerId(entity.id.Value);
                        damageEffect.AddTargetId(entity.targetId.Value);
                        damageEffect.AddTargetPoint(entity.targetPoint.Value);
                        damageEffect.isEffectCheckRequest = true;

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
