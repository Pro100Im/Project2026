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
            if (mapEntity == null) return;

            var spatialHash = mapEntity.spatialHash.Value;

            foreach (var attack in _attacks)
            {
                if (attack.trajectoryPathProgress.Value < 1)
                    continue;

                ApplySplashDamage(attack, spatialHash);

                attack.isDestructed = true;
            }
        }

        private void ApplySplashDamage(GameEntity attack, Dictionary<Vector2Int, List<int>> spatialHash)
        {
            var mainTarget = GetGameEntityById.Get(attack.targetId.Value);

            if (mainTarget == null || mainTarget.isDead) 
                return;

            CreateDamageRequest(attack, mainTarget.id.Value, attack.targetPoint.Value)
                .isDamageEffectRequest = true;

            var splashRadius = attack.areaAttack.Value;
            var sqrSplash = splashRadius * splashRadius;
            var impactPos = attack.targetPoint.Value;
            var originCell = mainTarget.currentCell.Value;
            var searchRange = Mathf.CeilToInt(splashRadius);

            _checkedTargets.Clear();
            _checkedTargets.Add(mainTarget.id.Value);

            for (var x = -searchRange; x <= searchRange; x++)
            {
                for (var y = -searchRange; y <= searchRange; y++)
                {
                    var checkPos = new Vector2Int(originCell.x + x, originCell.y + y);

                    if (spatialHash.TryGetValue(checkPos, out var cellUnits))
                    {
                        foreach (var unitId in cellUnits)
                        {
                            if (unitId == attack.ownerId.Value || !_checkedTargets.Add(unitId))
                                continue;

                            var other = GetGameEntityById.Get(unitId);

                            if (other == null || other.isDead || other.team.Value == attack.team.Value)
                                continue;

                            var otherPos = other.hasTransform ? other.transform.Value.position : (Vector3)other.currentCell.Value;
                            var distSqr = (otherPos - (Vector3)impactPos).sqrMagnitude;

                            if (distSqr <= sqrSplash)
                                CreateDamageRequest(attack, unitId, otherPos);
                        }
                    }
                }
            }
        }

        private GameEntity CreateDamageRequest(GameEntity attack, int targetId, Vector3 hitPoint)
        {
            var damage = CreateGameEntity.Empty();

            damage.AddOwnerId(attack.ownerId.Value);
            damage.AddTargetId(targetId);
            damage.AddTargetPoint(hitPoint);
            damage.AddTotalDamage(0);
            damage.isDamageRequest = true;

            return damage;
        }
    }
}