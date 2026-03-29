using Code.Game.StaticData.Configs;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Wave.Systems
{
    public class WaveStartSystem : IExecuteSystem
    {
        private readonly WavesConfig _wavesConfig;
        private readonly IGroup<GameEntity> _waves;
        private readonly IGroup<GameEntity> _waveRequsts;

        private readonly List<GameEntity> _buffer = new(5);

        public WaveStartSystem(GameContext gameContext, WavesConfig wavesConfig)
        {
            _wavesConfig = wavesConfig;

            _waves = gameContext.GetGroup(GameMatcher
              .AllOf(
              GameMatcher.CurrentWaveNumber,
              GameMatcher.WaveEnemiesAlive,
              GameMatcher.Cooldown,
              GameMatcher.CurrentWaveEnemies));

            _waveRequsts = gameContext.GetGroup(GameMatcher.WaveStartRequsted);
        }

        public void Execute()
        {
            foreach (var waveRequst in _waveRequsts.GetEntities(_buffer))
            {
                foreach (var wave in _waves)
                {
                    if(wave.currentWaveNumber.Value >= _wavesConfig.WaveDatas.Length)
                        continue;

                    wave.currentWaveEnemies.Value.AddRange(_wavesConfig.WaveDatas[wave.currentWaveNumber.Value].EntityConfigs);
                    wave.ReplaceCurrentWaveNumber(wave.currentWaveNumber.Value += 1);
                    wave.isWaveInProgress = true;
                }

                waveRequst.Destroy();
            }
        }
    }
}