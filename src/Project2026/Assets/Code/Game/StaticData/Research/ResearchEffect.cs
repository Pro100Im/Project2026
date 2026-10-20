using Entitas;

namespace Code.Game.StaticData.Research
{
    public abstract class ResearchEffect
    {
        public abstract void ApplyTown(TownEntity node);
        public abstract void ApplyGame(GameEntity node);
    }
}
