using Code.Game.Common.Entity;
using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Target.Systems
{
    public class SelectCombatTargetSystem : IExecuteSystem
    {
        private const float HoldRangeMultiplier = 1.15f;
        private const float SwitchCloserRatio = 0.7f;
        private const int RescanStaggerFrames = 15;

        private readonly IGroup<GameEntity> _attackers;
        private readonly IGroup<GameEntity> _maps;

        private readonly List<GameEntity> _buffer = new(256);
        private readonly HashSet<int> _processedTargets = new(256);

        public SelectCombatTargetSystem(GameContext context)
        {
            _attackers = context.GetGroup(GameMatcher.AllOf(
                GameMatcher.Id,
                GameMatcher.Team,
                GameMatcher.Range,
                GameMatcher.DetectionRange,
                GameMatcher.Transform,
                GameMatcher.CurrentCell));

            _maps = context.GetGroup(GameMatcher.AllOf(
                GameMatcher.SpatialHash,
                GameMatcher.TilemapMovement,
                GameMatcher.SurroundField));
        }

        public void Execute()
        {
            var mapEntity = _maps.GetSingleEntity();

            if (mapEntity == null)
                return;

            var spatialHash = mapEntity.spatialHash.Value;
            var tilemap = mapEntity.tilemapMovement.Value;
            var surroundField = mapEntity.surroundField.Value;
            var attackers = _attackers.GetEntities(_buffer);
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
                        || GetSqrDistToTarget(attackOriginPos, currentTarget, tilemap) > holdSqr)
                    {
                        if (!attacker.isAttacking)
                        {
                            ReleaseSurroundIfOwned(attacker, surroundField);
                            attacker.RemoveTargetId();
                        }
                    }
                    else
                    {
                        SyncSurroundToTargetId(attacker, surroundField);
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
                var attackerCell = attacker.currentCell.Value;
                var iRange = Mathf.CeilToInt(TargetService.GetEffectiveRange(detectionRange));

                _processedTargets.Clear();

                for (var x = -iRange; x <= iRange; x++)
                {
                    for (var y = -iRange; y <= iRange; y++)
                    {
                        var checkPos = new Vector2Int(attackerCell.x + x, attackerCell.y + y);

                        if (!spatialHash.TryGetValue(checkPos, out var potentialTargets))
                            continue;

                        for (var j = 0; j < potentialTargets.Count; j++)
                        {
                            var targetId = potentialTargets[j];

                            if (targetId == attacker.id.Value || !_processedTargets.Add(targetId))
                                continue;

                            var target = GetGameEntityById.Get(targetId);

                            if (!IsValidCombatTarget(target, myTeam))
                                continue;

                            var sDist = GetSqrDistToTarget(attackOriginPos, target, tilemap);

                            if (sDist > sqrPhysicalRange)
                                continue;

                            if (sDist < bestSqrDist)
                            {
                                bestSqrDist = sDist;
                                bestTargetId = targetId;
                            }
                        }
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
                        var currentSqr = GetSqrDistToTarget(attackOriginPos, currentTarget, tilemap);

                        if (bestSqrDist > currentSqr * SwitchCloserRatio)
                            continue;
                    }

                    if (attacker.isAttacking)
                        continue;
                }

                if (attacker.hasTargetId && attacker.targetId.Value != bestTargetId)
                    ReleaseSurroundIfOwned(attacker, surroundField);

                attacker.ReplaceTargetId(bestTargetId);
                SyncSurroundToTargetId(attacker, surroundField);
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

        private static float GetSqrDistToTarget(
            Vector2 attackOriginPos,
            GameEntity target,
            Dictionary<Vector3Int, Vector3> tilemap)
        {
            var targetPoint = TargetService.GetClosestPoint(target, tilemap, attackOriginPos);
            var dx = attackOriginPos.x - targetPoint.x;
            var dy = attackOriginPos.y - targetPoint.y;

            return (dx * dx) + (dy * dy);
        }

        private static void SyncSurroundToTargetId(GameEntity attacker, Dictionary<Vector3Int, int> surroundField)
        {
            if (!attacker.hasSurroundSlot || !attacker.hasSurroundTargetId || !attacker.hasTargetId)
                return;

            if (attacker.surroundTargetId.Value == attacker.targetId.Value)
                return;

            ReleaseSurroundIfOwned(attacker, surroundField);
        }

        private static void ReleaseSurroundIfOwned(GameEntity unit, Dictionary<Vector3Int, int> surroundField)
        {
            if (!unit.hasSurroundSlot)
            {
                if (unit.hasSurroundTargetId)
                    unit.RemoveSurroundTargetId();

                return;
            }

            var slot = unit.surroundSlot.Value;
            var unitId = unit.id.Value;

            if (surroundField.TryGetValue(slot, out var ownerId) && ownerId == unitId)
                surroundField.Remove(slot);

            unit.RemoveSurroundSlot();

            if (unit.hasSurroundTargetId)
                unit.RemoveSurroundTargetId();
        }
    }
}
