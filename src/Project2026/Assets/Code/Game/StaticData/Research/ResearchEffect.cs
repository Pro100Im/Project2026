using Entitas;
using System;

namespace Code.Game.StaticData.Research
{
    [Serializable]
    public abstract class ResearchEffect
    {
        public abstract void ApplyTown(TownEntity node);
        public abstract void ApplyGame(GameEntity node);
    }
}
