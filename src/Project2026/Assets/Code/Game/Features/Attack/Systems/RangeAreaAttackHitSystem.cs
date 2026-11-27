using Code.Game.Common.Entity;
using Code.Game.Features.Level;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Attack.Systems
{
    public class RangeAreaAttackHitSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _maps;
        private readonly IGroup<GameEntity> _attacks;

        private readonly List<GameEntity> _attacksBuffer = new(Code.Game.Features.Level.LevelBuffers.DefaultCapacity);

        public RangeAreaAttackHitSystem()
        {
            var gameContext = Contexts.sharedInstance.game;

            _maps = gameContext.GetGroup(GameMatcher.Arena);

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

            var arena = mapEntity.arena.Value;
            var attacks = _attacks.GetEntities(_attacksBuffer);

            for (int i = 0; i < attacks.Count; i++)
            {
                var attack = attacks[i];

                if (attack.trajectoryPathProgress.Value < 1)
                    continue;

                ApplySplashDamage(attack, arena);

                attack.isDestructed = true;
            }
        }

        private void ApplySplashDamage(GameEntity attack, LevelArena arena)
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
            var mainTargetId = mainTarget.id.Value;
            var originMin = arena.CellMin(originCell.x, originCell.y);
            var searchRange = Mathf.CeilToInt(splashRadius) * arena.CellSize;

            var candidates = arena.Query(
                originMin.x - searchRange,
                originMin.y - searchRange,
                originMin.x + arena.CellSize + searchRange,
                originMin.y + arena.CellSize + searchRange);

            for (var j = 0; j < candidates; j++)
            {
                var unitId = arena.QueryResults[j];

                if (unitId == attack.ownerId.Value || unitId == mainTargetId)
                    continue;

                var other = GetGameEntityById.Get(unitId);

                if (other == null || other.isDead || !other.hasTeam || other.team.Value == attack.team.Value)
                    continue;

                var otherPos = other.hasWoldPos ? other.woldPos.Value : (Vector3)other.currentCell.Value;
                var distSqr = (otherPos - (Vector3)impactPos).sqrMagnitude;

                if (distSqr <= sqrSplash)
                    CreateDamageRequest(attack, unitId, otherPos);
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
            damage.isAreaDamage = true;

            var damageEffect = CreateGameEntity.Empty();

            damageEffect.AddOwnerId(attack.id.Value);
            damageEffect.AddTargetId(targetId);
            damageEffect.AddTargetPoint(hitPoint);
            damageEffect.isEffectCheckRequest = true;

            return damage;
        }
    }
}