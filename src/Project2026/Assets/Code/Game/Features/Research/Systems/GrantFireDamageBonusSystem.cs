using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Research.Systems
{
    public class GrantFireDamageBonusSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _bonuses;
        private readonly IGroup<GameEntity> _attackers;
        private readonly List<GameEntity> _buffer = new(64);

        public GrantFireDamageBonusSystem()
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
            var value = bonus == null ? 0 : bonus.fireDamageBonus.Value;
            var attackers = _attackers.GetEntities(_buffer);

            for (var i = 0; i < attackers.Count; i++)
            {
                var attacker = attackers[i];

                if (attacker.isResearchNode || attacker.isResearchBonus)
                    continue;

                if (attacker.hasFireDamageBonus && attacker.fireDamageBonus.Value == value)
                    continue;

                if (attacker.hasFireDamageBonus)
                    attacker.ReplaceFireDamageBonus(value);
                else
                    attacker.AddFireDamageBonus(value);
            }
        }
    }
}
