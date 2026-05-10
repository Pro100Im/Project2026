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

        public CheckTargetSystem(GameContext context)
        {
            _attackers = context.GetGroup(GameMatcher.AllOf(
                GameMatcher.CurrentCell,
                GameMatcher.UnitSize,
                GameMatcher.Range,
                GameMatcher.Team,
                GameMatcher.Transform,
                GameMatcher.Id));

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
            var tilemapMovement = mapEntity.tilemapMovement.Value;

            foreach (var attacker in _attackers.GetEntities(_buffer))
            {
                var attackerPos = attacker.currentCell.Value;
                var size = attacker.unitSize.Value;
                var range = attacker.range.Value;
                var sqrRange = range * range;
                var myTeam = attacker.team.Value;

                var bestTargetId = -1;
                var bestTargetCell = attackerPos;
                var closestSqrDist = float.MaxValue;

                var iRange = Mathf.CeilToInt(range);

                for (var x = -iRange; x < size.x + iRange; x++)
                {
                    for (var y = -iRange; y < size.y + iRange; y++)
                    {
                        if (x >= 0 && x < size.x && y >= 0 && y < size.y) 
                            continue;

                        var checkPos = new Vector3Int(attackerPos.x + x, attackerPos.y + y, 0);
                        var checkPos2D = new Vector2Int(checkPos.x, checkPos.y);

                        if (spatialHash.TryGetValue(checkPos2D, out var potentialTargets))
                        {
                            foreach (var targetId in potentialTargets)
                            {
                                if (targetId == attacker.id.Value) 
                                    continue;

                                var target = GetGameEntityById.Get(targetId);

                                if (target != null && target.team.Value != myTeam && target.isTargetable && !target.isDead)
                                {
                                    var sDist = GetSqrDistanceToCell(attackerPos, size, checkPos);

                                    if (sDist <= sqrRange && sDist < closestSqrDist)
                                    {
                                        closestSqrDist = sDist;
                                        bestTargetId = targetId;
                                        bestTargetCell = checkPos;
                                    }
                                }
                            }
                        }
                    }
                }

                if (bestTargetId != -1)
                {
                    var attackerWorldPos = attacker.transform.Value.position;

                    if (tilemapMovement.TryGetValue(bestTargetCell, out var targetWorldPos))
                    {
                        attacker.ReplaceAttackerPoint(attackerWorldPos);
                        attacker.ReplaceTargetPoint(targetWorldPos);
                        attacker.ReplaceTargetCell(bestTargetCell);
                        attacker.ReplaceTargetId(bestTargetId);
                    }
                }
                else if (attacker.hasTargetId)
                {
                    attacker.RemoveTargetId();
                }
            }
        }

        private float GetSqrDistanceToCell(Vector3Int origin, Vector2Int size, Vector3Int cell)
        {
            var closestX = Mathf.Clamp(cell.x, origin.x, origin.x + size.x - 1);
            var closestY = Mathf.Clamp(cell.y, origin.y, origin.y + size.y - 1);
            var dx = cell.x - closestX;
            var dy = cell.y - closestY;

            return (dx * dx) + (dy * dy);
        }
    }
}