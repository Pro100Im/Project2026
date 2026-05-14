using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Target.Systems
{
    public class CheckTargetSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _attackers;
        private readonly IGroup<GameEntity> _maps;
        private readonly List<GameEntity> _buffer = new(128);
        private readonly HashSet<int> _processedTargets = new(128);

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

            foreach (var attacker in _attackers.GetEntities(_buffer))
            {
                var attackerWorldPos = attacker.transform.Value.position;
                var range = attacker.range.Value;
                var sqrRange = range * range;
                var myTeam = attacker.team.Value;
                var attackerCell = attacker.currentCell.Value;

                var bestTargetId = -1;
                var bestTargetPoint = Vector3.zero;
                var closestSqrDist = float.MaxValue;

                _processedTargets.Clear();

                var iRange = Mathf.CeilToInt(range);

                for (var x = -iRange; x <= iRange; x++)
                {
                    for (var y = -iRange; y <= iRange; y++)
                    {
                        var checkPos = new Vector2Int(attackerCell.x + x, attackerCell.y + y);

                        if (spatialHash.TryGetValue(checkPos, out var potentialTargets))
                        {
                            foreach (var targetId in potentialTargets)
                            {
                                if (targetId == attacker.id.Value || !_processedTargets.Add(targetId))
                                    continue;

                                var target = GetGameEntityById.Get(targetId);

                                if (target != null && target.team.Value != myTeam && target.isTargetable && !target.isDead && target.hasBounds)
                                {
                                    var targetBounds = target.bounds.Value.bounds;
                                    var closestPoint = targetBounds.ClosestPoint(attackerWorldPos);

                                    var dx = attackerWorldPos.x - closestPoint.x;
                                    var dy = attackerWorldPos.y - closestPoint.y;
                                    var sDist = (dx * dx) + (dy * dy);

                                    if (sDist <= sqrRange && sDist < closestSqrDist)
                                    {
                                        closestSqrDist = sDist;
                                        bestTargetId = targetId;
                                        bestTargetPoint = closestPoint;
                                    }
                                }
                            }
                        }
                    }
                }

                if (bestTargetId != -1)
                {
                    attacker.ReplaceAttackerPoint(attackerWorldPos);
                    attacker.ReplaceTargetPoint(bestTargetPoint);
                    attacker.ReplaceTargetId(bestTargetId);

                    if (attacker.hasTargetCell)
                        attacker.RemoveTargetCell();
                }
                else if (attacker.hasTargetId)
                {
                    attacker.RemoveTargetId();
                }
            }
        }
    }
}