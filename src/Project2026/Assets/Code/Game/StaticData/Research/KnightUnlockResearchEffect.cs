using Code.Game.StaticData.Configs;
using System;

namespace Code.Game.StaticData.Research
{
    [Serializable]
    public class KnightUnlockResearchEffect : ResearchEffect
    {
        public EntityConfig Config;
        public int Priority;

        public override void ApplyTown(TownEntity node) => node.AddResearchKnightUnlock(Config, Priority);

        public override void ApplyGame(GameEntity node) => node.AddResearchKnightUnlock(Config, Priority);
    }
}
