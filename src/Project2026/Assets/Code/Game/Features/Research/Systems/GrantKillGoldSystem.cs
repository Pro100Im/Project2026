using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Research.Systems
{
    public class GrantKillGoldSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _bonuses;
        private readonly IGroup<GameEntity> _requests;
        private readonly List<GameEntity> _buffer = new(16);

        public GrantKillGoldSystem()
        {
            var game = Contexts.sharedInstance.game;
            _bonuses = game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.ResearchBonus,
                    GameMatcher.GoldIncomeBonus));
            _requests = game.GetGroup(GameMatcher.ExchequerGoldChangeRequest);
        }

        public void Execute()
        {
            var bonus = _bonuses.GetSingleEntity();
            var multiplier = bonus == null ? 1f : 1f + bonus.goldIncomeBonus.Value;
            var requests = _requests.GetEntities(_buffer);

            for (var i = 0; i < requests.Count; i++)
            {
                var request = requests[i];

                if (request.isGoldIncomeScaled || request.exchequerGoldChangeRequest.Value <= 0)
                    continue;

                var scaled = Mathf.RoundToInt(request.exchequerGoldChangeRequest.Value * multiplier);
                request.ReplaceExchequerGoldChangeRequest(scaled);
                request.isGoldIncomeScaled = true;
            }
        }
    }
}
