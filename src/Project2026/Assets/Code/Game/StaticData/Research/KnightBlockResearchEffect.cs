using System;

namespace Code.Game.StaticData.Research
{
    [Serializable]
    public class KnightBlockResearchEffect : ResearchEffect
    {
        public float Value;

        public override void ApplyTown(TownEntity node) => node.AddResearchBlockBonus(Value);

        public override void ApplyGame(GameEntity node) => node.AddResearchBlockBonus(Value);
    }
}
