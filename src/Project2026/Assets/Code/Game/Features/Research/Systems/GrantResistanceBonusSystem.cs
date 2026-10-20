using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Research.Systems
{
    public class GrantResistanceBonusSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _bonuses;
        private readonly IGroup<GameEntity> _defenders;
        private readonly List<GameEntity> _buffer = new(64);

        public GrantResistanceBonusSystem()
        {
            var game = Contexts.sharedInstance.game;
            _bonuses = game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.ResearchBonus,
                    GameMatcher.ResistanceBonus));
            _defenders = game.GetGroup(GameMatcher
                .AllOf(GameMatcher.Team)
                .AnyOf(
                    GameMatcher.PhysicalResistance,
                    GameMatcher.FireResistance,
                    GameMatcher.FrostResistance));
        }

        public void Execute()
        {
            var bonus = _bonuses.GetSingleEntity();
            var value = bonus == null ? 0 : bonus.resistanceBonus.Value;
            var defenders = _defenders.GetEntities(_buffer);

            for (var i = 0; i < defenders.Count; i++)
            {
                var defender = defenders[i];

                if (defender.team.Value != Team.Player || defender.isResearchBonus)
                    continue;

                if (defender.hasResistanceBonus && defender.resistanceBonus.Value == value)
                    continue;

                if (defender.hasResistanceBonus)
                    defender.ReplaceResistanceBonus(value);
                else
                    defender.AddResistanceBonus(value);
            }
        }
    }
}
