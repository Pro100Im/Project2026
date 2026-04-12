namespace Code.Game.Common.Random
{
    public class RandomService : IRandomService
    {
        public int GetGlobalRandom(int min, int max)
            => UnityEngine.Random.Range(min, max);

        public float GetGlobalRandom(float min, float max)
            => UnityEngine.Random.Range(min, max);

        public int GetLocalRandom(int min, int max, int seed)
        {
            var rng = new System.Random(seed);
            var value = rng.Next(min, max);

            return value;
        }

        public float GetLocalRandom(float min, float max, int seed)
        {
            var rng = new System.Random(seed);
            double value = rng.NextDouble();

            return (float)(min + value * (max - min));
        }
    }
}