using Code.Game.Common.Entity;
using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Target.Systems
{
    public class CheckTargetSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _attackers;
        private readonly IGroup<GameEntity> _maps;

        private readonly List<GameEntity> _buffer = new(256);
        private readonly HashSet<int> _processedTargets = new(256);

        public CheckTargetSystem(GameContext context)
        {
            _attackers = context.GetGroup(GameMatcher.AllOf(
                GameMatcher.Id,
                GameMatcher.Team,
                GameMatcher.Range,
                GameMatcher.Transform,
                GameMatcher.CurrentCell));

            _maps = context.GetGroup(GameMatcher.AllOf(
                GameMatcher.SpatialHash,
                GameMatcher.TilemapMovement));
        }

        public void Execute()
        {
            var mapEntity = _maps.GetSingleEntity();

            if (mapEntity == null)
                return;

            var spatialHash = mapEntity.spatialHash.Value;
            var attackers = _attackers.GetEntities(_buffer);

            for (var i = 0; i < attackers.Count; i++)
            {
                var attacker = attackers[i];
                var basePos = attacker.woldPos.Value;
                var attackOriginPos = basePos;

                if (attacker.hasUnitAnchorPoint)
                    attackOriginPos += attacker.unitAnchorPoint.Value;

                var range = attacker.range.Value;
                var myTeam = attacker.team.Value;
                var attackerCell = attacker.currentCell.Value;
                var physicalRange = TargetService.GetPhysicalRange(range);
                var sqrPhysicalRange = physicalRange * physicalRange;

                var bestTargetId = -1;
                var bestTargetPoint = Vector3.zero;
                var closestSqrDist = float.MaxValue;

                _processedTargets.Clear();

                var iRange = Mathf.CeilToInt(TargetService.GetEffectiveRange(range));

                for (var x = -iRange; x <= iRange; x++)
                {
                    for (var y = -iRange; y <= iRange; y++)
                    {
                        var checkPos = new Vector2Int(attackerCell.x + x, attackerCell.y + y);

                        if (spatialHash.TryGetValue(checkPos, out var potentialTargets))
                        {
                            for (var j = 0; j < potentialTargets.Count; j++)
                            {
                                var targetId = potentialTargets[j];

                                if (targetId == attacker.id.Value || !_processedTargets.Add(targetId))
                                    continue;

                                var target = GetGameEntityById.Get(targetId);

                                if (target != null && target.team.Value != myTeam && target.isTargetable && !target.isDead)
                                {
                                    if (!target.hasCurrentCell)
                                        continue;

                                    var targetPoint = TargetService.GetClosestPoint(target, attackOriginPos);

                                    var dx = attackOriginPos.x - targetPoint.x;
                                    var dy = attackOriginPos.y - targetPoint.y;
                                    var sDist = (dx * dx) + (dy * dy);

                                    if (sDist > sqrPhysicalRange)
                                        continue;

                                    if (sDist < closestSqrDist)
                                    {
                                        closestSqrDist = sDist;
                                        bestTargetId = targetId;
                                        bestTargetPoint = targetPoint;
                                    }
                                }
                            }
                        }
                    }
                }

                if (bestTargetId != -1)
                {
                    attacker.ReplaceAttackerPoint(attackOriginPos);
                    attacker.ReplaceTargetPoint(bestTargetPoint);
                    attacker.ReplaceTargetId(bestTargetId);
                }
                else if (attacker.hasTargetId)
                {
                    attacker.RemoveTargetId();
                }
            }
        }
    }
}