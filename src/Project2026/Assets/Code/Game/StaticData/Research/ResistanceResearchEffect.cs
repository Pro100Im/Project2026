using System;

namespace Code.Game.StaticData.Research
{
    [Serializable]
    public class ResistanceResearchEffect : ResearchEffect
    {
        public int Value;

        public override void ApplyTown(TownEntity node) => node.AddResearchResistanceBonus(Value);

        public override void ApplyGame(GameEntity node) => node.AddResearchResistanceBonus(Value);
    }
}
