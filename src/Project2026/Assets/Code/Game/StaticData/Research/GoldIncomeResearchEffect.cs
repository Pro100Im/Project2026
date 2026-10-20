using System;

namespace Code.Game.StaticData.Research
{
    [Serializable]
    public class GoldIncomeResearchEffect : ResearchEffect
    {
        public float Value;

        public override void ApplyTown(TownEntity node) => node.AddResearchGoldBonus(Value);

        public override void ApplyGame(GameEntity node) => node.AddResearchGoldBonus(Value);
    }
}
