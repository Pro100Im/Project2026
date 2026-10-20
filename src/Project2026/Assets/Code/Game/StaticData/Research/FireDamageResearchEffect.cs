using System;

namespace Code.Game.StaticData.Research
{
    [Serializable]
    public class FireDamageResearchEffect : ResearchEffect
    {
        public float Value;

        public override void ApplyTown(TownEntity node) => node.AddResearchFireBonus(Value);

        public override void ApplyGame(GameEntity node) => node.AddResearchFireBonus(Value);
    }
}
