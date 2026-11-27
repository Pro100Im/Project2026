using Code.Game.Common.Entity;
using Code.Game.Features.Level;
using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Target.Systems
{
    public class SelectCombatTargetSystem : IExecuteSystem
    {
        // TO DO
        private const float HoldRangeMultiplier = 1.15f;
        private const float SwitchCloserRatio = 0.7f;
        private const int RescanStaggerFrames = 15;

        private readonly IGroup<GameEntity> _attackers;
        private readonly IGroup<GameEntity> _maps;

        private readonly List<GameEntity> _buffer = new(LevelBuffers.DefaultCapacity);

        public SelectCombatTargetSystem()
        {
            var gameContext = Contexts.sharedInstance.game;

            _attackers = gameContext.GetGroup(GameMatcher.AllOf(
                GameMatcher.Id,
                GameMatcher.Team,
                GameMatcher.Range,
                GameMatcher.DetectionRange,
                GameMatcher.Transform,
                GameMatcher.CurrentCell));

            _maps = gameContext.GetGroup(GameMatcher.Arena);
        }

        public void Execute()
        {
            var mapEntity = _maps.GetSingleEntity();

            if (mapEntity == null)
                return;

            var arena = mapEntity.arena.Value;
            var attackers = _attackers.GetEntities(LevelBuffers.Fit(_buffer, arena));
            var frame = Time.frameCount;

            for (var i = 0; i < attackers.Count; i++)
            {
                var attacker = attackers[i];

                if (attacker.isDead)
                    continue;

                var attackOriginPos = GetAttackOrigin(attacker);
                var myTeam = attacker.team.Value;
                var detectionRange = attacker.detectionRange.Value;
                var holdPhysical = TargetService.GetPhysicalRange(detectionRange) * HoldRangeMultiplier;
                var holdSqr = holdPhysical * holdPhysical;

                if (attacker.hasTargetId)
                {
                    var currentTarget = GetGameEntityById.Get(attacker.targetId.Value);

                    if (!IsValidCombatTarget(currentTarget, myTeam)
                        || GetSqrDistToTarget(attackOriginPos, currentTarget, arena) > holdSqr)
                    {
                        if (!attacker.isAttacking)
                        {
                            ReleaseSurroundIfOwned(attacker, arena);
                            attacker.RemoveTargetId();
                        }
                    }
                    else
                    {
                        SyncSurroundToTargetId(attacker, arena);
                    }
                }

                var shouldRescan = !attacker.hasTargetId
                    || (!attacker.isAttacking
                        && (frame + attacker.id.Value) % RescanStaggerFrames == 0);

                if (!shouldRescan)
                    continue;

                var bestTargetId = -1;
                var bestSqrDist = float.MaxValue;
                var physicalRange = TargetService.GetPhysicalRange(detectionRange);
                var sqrPhysicalRange = physicalRange * physicalRange;
                var scanHalf = (Mathf.CeilToInt(TargetService.GetEffectiveRange(detectionRange)) + 1.5f) * arena.CellSize;

                var candidates = arena.Query(
                    attackOriginPos.x - scanHalf,
                    attackOriginPos.y - scanHalf,
                    attackOriginPos.x + scanHalf,
                    attackOriginPos.y + scanHalf);

                for (var j = 0; j < candidates; j++)
                {
                    var targetId = arena.QueryResults[j];

                    if (targetId == attacker.id.Value)
                        continue;

                    var target = GetGameEntityById.Get(targetId);

                    if (!IsValidCombatTarget(target, myTeam))
                        continue;

                    var sDist = GetSqrDistToTarget(attackOriginPos, target, arena);

                    if (sDist > sqrPhysicalRange)
                        continue;

                    if (sDist < bestSqrDist)
                    {
                        bestSqrDist = sDist;
                        bestTargetId = targetId;
                    }
                }

                if (bestTargetId == -1)
                    continue;

                if (attacker.hasTargetId)
                {
                    var currentId = attacker.targetId.Value;

                    if (currentId == bestTargetId)
                        continue;

                    var currentTarget = GetGameEntityById.Get(currentId);

                    if (IsValidCombatTarget(currentTarget, myTeam))
                    {
                        var currentSqr = GetSqrDistToTarget(attackOriginPos, currentTarget, arena);

                        if (bestSqrDist > currentSqr * SwitchCloserRatio)
                            continue;
                    }

                    if (attacker.isAttacking)
                        continue;
                }

                if (attacker.hasTargetId && attacker.targetId.Value != bestTargetId)
                    ReleaseSurroundIfOwned(attacker, arena);

                attacker.ReplaceTargetId(bestTargetId);
                SyncSurroundToTargetId(attacker, arena);
            }
        }

        private static Vector2 GetAttackOrigin(GameEntity attacker)
        {
            var origin = attacker.woldPos.Value;

            if (attacker.hasUnitAnchorPoint)
                origin += attacker.unitAnchorPoint.Value;

            return origin;
        }

        private static bool IsValidCombatTarget(GameEntity target, Team myTeam)
        {
            return target != null
                && target.hasTeam
                && target.team.Value != myTeam
                && target.isTargetable
                && !target.isDead
                && target.hasCurrentCell;
        }

        private static float GetSqrDistToTarget(Vector2 attackOriginPos, GameEntity target, LevelArena arena)
        {
            var targetPoint = TargetService.GetClosestPoint(target, arena, attackOriginPos);
            var dx = attackOriginPos.x - targetPoint.x;
            var dy = attackOriginPos.y - targetPoint.y;

            return (dx * dx) + (dy * dy);
        }

        private static void SyncSurroundToTargetId(GameEntity attacker, LevelArena arena)
        {
            if (!attacker.hasSurroundSlot || !attacker.hasSurroundTargetId || !attacker.hasTargetId)
                return;

            if (attacker.surroundTargetId.Value == attacker.targetId.Value)
                return;

            ReleaseSurroundIfOwned(attacker, arena);
        }

        private static void ReleaseSurroundIfOwned(GameEntity unit, LevelArena arena)
        {
            if (!unit.hasSurroundSlot)
            {
                if (unit.hasSurroundTargetId)
                    unit.RemoveSurroundTargetId();

                return;
            }

            var slot = unit.surroundSlot.Value;
            var unitId = unit.id.Value;

            arena.ReleaseSurround(slot, unitId);

            unit.RemoveSurroundSlot();

            if (unit.hasSurroundTargetId)
                unit.RemoveSurroundTargetId();
        }
    }
}
