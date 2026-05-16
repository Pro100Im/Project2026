using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Attack.Systems
{
    public class RangeAreaAttackHitSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _maps;
        private readonly IGroup<GameEntity> _attacks;

        private readonly List<GameEntity> _buffer = new(64);
        private readonly HashSet<int> _checkedTargets = new(128);

        public RangeAreaAttackHitSystem(GameContext gameContext)
        {
            _maps = gameContext.GetGroup(GameMatcher.AllOf(GameMatcher.SpatialHash));

            _attacks = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.OwnerId,
                    GameMatcher.TargetId,
                    GameMatcher.Team,
                    GameMatcher.TrajectoryPathProgress,
                    GameMatcher.AreaAttack));
        }

        public void Execute()
        {
            var mapEntity = _maps.GetSingleEntity();

            if (mapEntity == null)
                return;

            var spatialHash = mapEntity.spatialHash.Value;

            foreach (var attack in _attacks.GetEntities(_buffer))
            {
                if (attack.trajectoryPathProgress.Value < 1)
                    continue;

                ApplySplashDamage(attack, spatialHash);

                attack.isDestructed = true;
            }
        }

        private void ApplySplashDamage(GameEntity attack, Dictionary<Vector2Int, List<int>> spatialHash)
        {
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

            var impactPoint = attack.targetPoint.Value;
            var splashRadius = attack.areaAttack.Value;
            var sqrSplash = splashRadius * splashRadius;

            var centerCellX = Mathf.RoundToInt(impactPoint.x);
            var centerCellY = Mathf.RoundToInt(impactPoint.y);
            var iRadius = Mathf.CeilToInt(splashRadius);

            _checkedTargets.Clear();

            for (var x = -iRadius; x <= iRadius; x++)
            {
                for (var y = -iRadius; y <= iRadius; y++)
                {
                    var checkPos = new Vector2Int(centerCellX + x, centerCellY + y);

                    if (spatialHash.TryGetValue(checkPos, out var potentialTargets))
                    {
                        foreach (var targetId in potentialTargets)
                        {
                            if (targetId == attack.ownerId.Value || targetId == attack.targetId.Value || _checkedTargets.Contains(targetId))
                                continue;

                            var otherTarget = GetGameEntityById.Get(targetId);

                            if (otherTarget != null && otherTarget.team.Value != attack.team.Value && !otherTarget.isDead && otherTarget.isTargetable && otherTarget.hasBounds)
                            {
                                _checkedTargets.Add(targetId);

                                var targetBounds = otherTarget.bounds.Value.bounds;
                                var closestPoint = targetBounds.ClosestPoint(impactPoint);

                                var dx = impactPoint.x - closestPoint.x;
                                var dy = impactPoint.y - closestPoint.y;
                                var sDist = (dx * dx) + (dy * dy);

                                if (sDist <= sqrSplash)
                                {
                                    var damage = CreateGameEntity.Empty();

                                    damage.AddOwnerId(attack.ownerId.Value);
                                    damage.AddTargetId(targetId);
                                    damage.AddTargetPoint(closestPoint);
                                    damage.AddTotalDamage(0);
                                    damage.isDamageRequest = true;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}