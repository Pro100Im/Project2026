using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Target.Systems
{
    public class CheckTargetSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _units;
        private readonly IGroup<GameEntity> _maps;

        private readonly List<GameEntity> _buffer = new(86);

        public CheckTargetSystem(GameContext context)
        {
            _units = context.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.CurrentCell,
                GameMatcher.UnitSize,
                GameMatcher.Range,
                GameMatcher.Team,
                GameMatcher.Transform));

            _maps = context.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.OccupField));
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();

            if (map == null || !map.hasOccupField || !map.hasTilemapMovement)
                return;

            var occupField = map.occupField.Value;
            var tilemapMovement = map.tilemapMovement.Value;

            foreach (var attacker in _units.GetEntities(_buffer))
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

                        var checkPos = new Vector3Int(attackerPos.x + x, attackerPos.y + y);

                        if (occupField.TryGetValue(checkPos, out int entityId))
                        {
                            var sDist = GetSqrDistanceToCell(attackerPos, size, checkPos);

                            if (sDist <= sqrRange)
                            {
                                var target = GetGameEntityById.Get(entityId);

                                if (target != null && target.team.Value != myTeam && target.isTargetable)
                                {
                                    if (sDist < closestSqrDist)
                                    {
                                        closestSqrDist = sDist;
                                        bestTargetId = entityId;
                                        bestTargetCell = checkPos;

                                        break;
                                    }
                                }
                            }
                            else if (sDist > sqrRange && attacker.hasTargetId)
                            {
                                attacker.RemoveTargetId();
                            }
                        }
                    }

                    if (bestTargetId != -1)
                    {
                        var attackerWorldPos = attacker.transform.Value.position;
                        var targetWorldPos = tilemapMovement[bestTargetCell];

                        attacker.ReplaceAttackerPoint(attackerWorldPos);
                        attacker.ReplaceTargetPoint(targetWorldPos);
                        attacker.ReplaceTargetCell(bestTargetCell);
                        attacker.ReplaceTargetId(bestTargetId);
                    }
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