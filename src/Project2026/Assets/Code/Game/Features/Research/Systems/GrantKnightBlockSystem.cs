using Code.Game.Features.Unit;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Research.Systems
{
    public class GrantKnightBlockSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _bonuses;
        private readonly IGroup<GameEntity> _knights;
        private readonly List<GameEntity> _buffer = new(32);

        public GrantKnightBlockSystem()
        {
            var game = Contexts.sharedInstance.game;
            _bonuses = game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.ResearchBonus,
                    GameMatcher.KnightBlockBonus));
            _knights = game.GetGroup(GameMatcher.UnitKind);
        }

        public void Execute()
        {
            var bonus = _bonuses.GetSingleEntity();
            var chance = bonus == null ? 0f : bonus.knightBlockBonus.Value;
            var knights = _knights.GetEntities(_buffer);

            for (var i = 0; i < knights.Count; i++)
            {
                var knight = knights[i];

                if (!knight.isUnit || knight.unitKind.Value != UnitKind.Knight)
                    continue;

                if (knight.hasPassiveBlockChance && knight.passiveBlockChance.Value == chance)
                    continue;

                if (knight.hasPassiveBlockChance)
                    knight.ReplacePassiveBlockChance(chance);
                else
                    knight.AddPassiveBlockChance(chance);
            }
        }
    }
}
