using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Research.Systems
{
    public class GrantFireDamageMultiplierSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _bonuses;
        private readonly IGroup<GameEntity> _attackers;
        private readonly List<GameEntity> _buffer = new(64);

        public GrantFireDamageMultiplierSystem()
        {
            var game = Contexts.sharedInstance.game;
            _bonuses = game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.ResearchBonus,
                    GameMatcher.FireDamageBonus));
            _attackers = game.GetGroup(GameMatcher.FireDamage);
        }

        public void Execute()
        {
            var bonus = _bonuses.GetSingleEntity();
            var multiplier = bonus == null ? 1f : 1f + bonus.fireDamageBonus.Value;
            var attackers = _attackers.GetEntities(_buffer);

            for (var i = 0; i < attackers.Count; i++)
            {
                var attacker = attackers[i];

                if (attacker.isResearchNode || attacker.isResearchBonus)
                    continue;

                if (attacker.hasFireDamageMultiplier && attacker.fireDamageMultiplier.Value == multiplier)
                    continue;

                if (attacker.hasFireDamageMultiplier)
                    attacker.ReplaceFireDamageMultiplier(multiplier);
                else
                    attacker.AddFireDamageMultiplier(multiplier);
            }
        }
    }
}
