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

        public AbilityCastSystem()
        {
            var gameContext = Contexts.sharedInstance.game;

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
                .AllOf(GameMatcher.Arena));
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
            var arena = map.arena.Value;
            var center = cast.targetPoint.Value;
            var physicalRadius = cast.abilityRadius.Value * TargetService.CellSize;
            var sqrRadius = physicalRadius * physicalRadius;

            // Units are registered by cell footprint while their sprite position is offset from it,
            // so the search box is padded by a couple of cells.
            var searchRange = physicalRadius + (arena.CellSize * 2f);

            var candidates = arena.Query(
                center.x - searchRange,
                center.y - searchRange,
                center.x + searchRange,
                center.y + searchRange);

            for (var j = 0; j < candidates; j++)
                TryApplyToTarget(cast, arena.QueryResults[j], sqrRadius);
        }

        private void TryApplyToTarget(GameEntity cast, int targetId, float sqrRadius)
        {
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

        private bool MatchesTargetFilter(Team casterTeam, Team targetTeam, AbilityTargetType targetType)
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
    }
}
