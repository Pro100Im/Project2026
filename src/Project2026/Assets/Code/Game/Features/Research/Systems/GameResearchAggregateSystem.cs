using Code.Game.Common.Entity;
using Code.Game.StaticData.Configs;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Research.Systems
{
    public class GameResearchAggregateSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _nodes;
        private readonly IGroup<GameEntity> _bonuses;
        private readonly List<GameEntity> _buffer = new(32);

        public GameResearchAggregateSystem()
        {
            var game = Contexts.sharedInstance.game;
            _nodes = game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.ResearchNode,
                    GameMatcher.ResearchLevel));
            _bonuses = game.GetGroup(GameMatcher.ResearchBonus);
        }

        public void Execute()
        {
            var bonus = BonusEntity();
            var nodes = _nodes.GetEntities(_buffer);

            var fire = 0f;
            var resistance = 0;
            var gold = 0f;
            var block = 0f;
            EntityConfig knight = null;
            var knightPriority = int.MinValue;

            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];

                if (node.researchLevel.Value <= 0)
                    continue;

                if (node.hasResearchFireBonus)
                    fire += node.researchFireBonus.Value;

                if (node.hasResearchResistanceBonus)
                    resistance += node.researchResistanceBonus.Value;

                if (node.hasResearchGoldBonus)
                    gold += node.researchGoldBonus.Value;

                if (node.hasResearchBlockBonus)
                    block += node.researchBlockBonus.Value;

                if (!node.hasResearchKnightUnlock || node.researchKnightUnlock.Priority <= knightPriority)
                    continue;

                knightPriority = node.researchKnightUnlock.Priority;
                knight = node.researchKnightUnlock.Config;
            }

            SetFloat(bonus.hasFireDamageBonus, fire, bonus.hasFireDamageBonus ? bonus.fireDamageBonus.Value : 0f, bonus.AddFireDamageBonus, bonus.ReplaceFireDamageBonus);
            SetInt(bonus.hasResistanceBonus, resistance, bonus.hasResistanceBonus ? bonus.resistanceBonus.Value : 0, bonus.AddResistanceBonus, bonus.ReplaceResistanceBonus);
            SetFloat(bonus.hasGoldIncomeBonus, gold, bonus.hasGoldIncomeBonus ? bonus.goldIncomeBonus.Value : 0f, bonus.AddGoldIncomeBonus, bonus.ReplaceGoldIncomeBonus);
            SetFloat(bonus.hasKnightBlockBonus, block, bonus.hasKnightBlockBonus ? bonus.knightBlockBonus.Value : 0f, bonus.AddKnightBlockBonus, bonus.ReplaceKnightBlockBonus);

            if (knight == null)
            {
                if (bonus.hasUnlockedKnight)
                    bonus.RemoveUnlockedKnight();

                return;
            }

            if (!bonus.hasUnlockedKnight || bonus.unlockedKnight.Value != knight)
            {
                if (bonus.hasUnlockedKnight)
                    bonus.ReplaceUnlockedKnight(knight);
                else
                    bonus.AddUnlockedKnight(knight);
            }
        }

        private GameEntity BonusEntity()
        {
            var bonus = _bonuses.GetSingleEntity();

            if (bonus != null)
                return bonus;

            bonus = CreateGameEntity.Empty();
            bonus.isResearchBonus = true;
            return bonus;
        }

        private static void SetFloat(bool has, float value, float current, System.Action<float> add, System.Action<float> replace)
        {
            if (!has)
            {
                add(value);
                return;
            }

            if (current != value)
                replace(value);
        }

        private static void SetInt(bool has, int value, int current, System.Action<int> add, System.Action<int> replace)
        {
            if (!has)
            {
                add(value);
                return;
            }

            if (current != value)
                replace(value);
        }
    }
}
