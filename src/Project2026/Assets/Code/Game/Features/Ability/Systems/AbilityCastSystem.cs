using Code.Game.Common.Entity;
using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Ability.Systems
{
    public class AbilityCastSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _casts;
        private readonly IGroup<GameEntity> _maps;

        private readonly List<GameEntity> _castsBuffer = new(8);
        private readonly HashSet<int> _checkedTargets = new(128);

        public AbilityCastSystem(GameContext gameContext)
        {
            _casts = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.AbilityCastRequest,
                    GameMatcher.AbilityRadius,
                    GameMatcher.AbilityTargetFilter,
                    GameMatcher.FreezeEffect,
                    GameMatcher.TargetPoint,
                    GameMatcher.Team,
                    GameMatcher.Id)
                .NoneOf(GameMatcher.Destructed));

            _maps = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.SpatialHash,
                    GameMatcher.TilemapMovement));
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();

            if (map == null)
                return;

            var casts = _casts.GetEntities(_castsBuffer);

            for (var i = 0; i < casts.Count; i++)
            {
                var cast = casts[i];

                ApplyAbility(cast, map);
                cast.isDestructed = true;
            }
        }

        private void ApplyAbility(GameEntity cast, GameEntity map)
        {
            var spatialHash = map.spatialHash.Value;
            var originCell = FindClosestCell(cast.targetPoint.Value, map.tilemapMovement.Value);
            var searchRange = Mathf.CeilToInt(cast.abilityRadius.Value);
            var physicalRadius = cast.abilityRadius.Value * TargetService.CellSize;
            var sqrRadius = physicalRadius * physicalRadius;

            _checkedTargets.Clear();

            for (var x = -searchRange; x <= searchRange; x++)
            {
                for (var y = -searchRange; y <= searchRange; y++)
                {
                    var cell = new Vector2Int(originCell.x + x, originCell.y + y);

                    if (!spatialHash.TryGetValue(cell, out var unitIds))
                        continue;

                    for (var j = 0; j < unitIds.Count; j++)
                        TryApplyToTarget(cast, unitIds[j], sqrRadius);
                }
            }
        }

        private void TryApplyToTarget(GameEntity cast, int targetId, float sqrRadius)
        {
            if (!_checkedTargets.Add(targetId))
                return;

            var target = GetGameEntityById.Get(targetId);

            if (target == null || target.isDead || !target.isUnit || !target.hasTeam || !target.hasWoldPos)
                return;

            if (!MatchesTargetFilter(cast.team.Value, target.team.Value, cast.abilityTargetFilter.Value))
                return;

            if ((target.woldPos.Value - (Vector3)cast.targetPoint.Value).sqrMagnitude > sqrRadius)
                return;

            var request = CreateGameEntity.Empty();

            request.AddOwnerId(cast.id.Value);
            request.AddTargetId(target.id.Value);
            request.AddTargetPoint(target.woldPos.Value);
            request.isEffectCheckRequest = true;
        }

        private bool MatchesTargetFilter(
            Team casterTeam,
            Team targetTeam,
            AbilityTargetType targetType)
        {
            if (casterTeam == Team.None || targetTeam == Team.None)
                return false;

            return targetType switch
            {
                AbilityTargetType.Enemies => casterTeam != targetTeam,
                AbilityTargetType.Allies => casterTeam == targetTeam,
                AbilityTargetType.All => true,
                _ => false
            };
        }

        private Vector3Int FindClosestCell(
            Vector3 point,
            Dictionary<Vector3Int, Vector3> tilemap)
        {
            var closestCell = Vector3Int.zero;
            var closestDistance = float.MaxValue;

            foreach (var pair in tilemap)
            {
                var distance = (pair.Value - point).sqrMagnitude;

                if (distance >= closestDistance)
                    continue;

                closestDistance = distance;
                closestCell = pair.Key;
            }

            return closestCell;
        }
    }
}
