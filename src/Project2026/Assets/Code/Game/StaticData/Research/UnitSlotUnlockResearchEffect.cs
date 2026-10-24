using System;

namespace Code.Game.StaticData.Research
{
    [Serializable]
    public class UnitSlotUnlockResearchEffect : ResearchEffect
    {
        public UnitKind Kind;

        public override void ApplyTown(TownEntity node) => node.AddResearchUnitSlotUnlock(Kind);

        public override void ApplyGame(GameEntity node) => node.AddResearchUnitSlotUnlock(Kind);
    }
}
