namespace Code.Game.Common.Random
{
    public interface IRandomService
    {
        public int GetGlobalRandom(int min, int max);
        public float GetGlobalRandom(float min, float max);

        public int GetLocalRandom(int min, int max, int seed);
        public float GetLocalRandom(float min, float max, int seed);
    }
}