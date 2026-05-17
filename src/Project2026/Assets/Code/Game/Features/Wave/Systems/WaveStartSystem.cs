using Code.Game.StaticData.Configs;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Wave.Systems
{
    public class WaveStartSystem : IExecuteSystem
    {
        private readonly WavesConfig _wavesConfig;
        private readonly IGroup<GameEntity> _waves;
        private readonly IGroup<GameEntity> _waveRequests;

        private readonly List<GameEntity> _waveRequestsBuffer = new(16);
        private readonly List<GameEntity> _waveBuffer = new(16);

        public WaveStartSystem(GameContext gameContext, WavesConfig wavesConfig)
        {
            _wavesConfig = wavesConfig;

            _waves = gameContext.GetGroup(GameMatcher
              .AllOf(
              GameMatcher.CurrentWaveNumber,
              GameMatcher.WaveEnemiesAlive,
              GameMatcher.Cooldown,
              GameMatcher.CurrentWaveEnemies));

            _waveRequests = gameContext.GetGroup(GameMatcher.WaveStartRequsted);
        }

        public void Execute()
        {
            var waveRequsts = _waveRequests.GetEntities(_waveRequestsBuffer);

            for (var i = 0; i < waveRequsts.Count; i++)
            {
                var waveRequst = waveRequsts[i];
                var waves = _waves.GetEntities(_waveBuffer);

                for (var j = 0; j < waves.Count; j++)
                {
                    var wave = waves[j];

                    if (wave.currentWaveNumber.Value >= _wavesConfig.WaveDatas.Length)
                        continue;

                    wave.currentWaveEnemies.Value.AddRange(_wavesConfig.WaveDatas[wave.currentWaveNumber.Value].EntityConfigs);
                    wave.ReplaceCurrentWaveNumber(wave.currentWaveNumber.Value += 1);
                    wave.isWaveInProgress = true;
                }

                waveRequst.isDestructed = true;
            }
        }
    }
}