using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Attack.Systems
{
    public class RangeAttackHitSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _attacks;
        private readonly List<GameEntity> _attacksBuffer = new(86);

        public RangeAttackHitSystem(GameContext gameContext)
        {
            _attacks = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.OwnerId,
                    GameMatcher.TargetId,
                    GameMatcher.TrajectoryPathProgress));
        }

        public void Execute()
        {
            var attacks = _attacks.GetEntities(_attacksBuffer);

            for (var i = 0; i < attacks.Count; i++)
            {
                var attack = attacks[i];

                if (attack.hasAreaAttack || attack.trajectoryPathProgress.Value < 1)
                    continue;

                var target = GetGameEntityById.Get(attack.targetId.Value);

                if (!target.isDead)
                {
                    var damage = CreateGameEntity.Empty();

                    damage.AddOwnerId(attack.ownerId.Value);
                    damage.AddTargetId(attack.targetId.Value);
                    damage.AddTargetPoint(attack.targetPoint.Value);
                    damage.AddTotalDamage(0);
                    damage.isDamageRequest = true;
                    damage.isDamageEffectRequest = true;
                }
                
                attack.isDestructed = true;
            }
        }
    }
}