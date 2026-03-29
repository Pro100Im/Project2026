using Code.Common.Entity;
using Entitas;

namespace Code.Game.Features.Wave.Systems
{
    public class WaveInitSystem : IInitializeSystem
    {
        public void Initialize()
        {
            var entity = CreateGameEntity.Empty();

            entity.AddCurrentWaveNumber(0);
            entity.AddWaveEnemiesAlive(0);
            entity.AddCooldown(0);
            entity.AddCurrentWaveEnemies(new());
            entity.isWaveInProgress = false;
        }
    }
}