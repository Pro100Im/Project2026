using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Wave.Systems
{
    public class WaveStartSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _waves;
        private readonly IGroup<GameEntity> _waveRequests;

        private readonly List<GameEntity> _waveRequestsBuffer = new(16);
        private readonly List<GameEntity> _waveBuffer = new(16);

        public WaveStartSystem()
        {
            var gameContext = Contexts.sharedInstance.game;

            _waves = gameContext.GetGroup(GameMatcher
              .AllOf(
              GameMatcher.CurrentWaveNumber,
              GameMatcher.WaveEnemiesAlive,
              GameMatcher.Cooldown,
              GameMatcher.CurrentWaveEnemies,
              GameMatcher.WaveSchedule));

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
                    var waveDatas = wave.waveSchedule.Value.WaveDatas;

                    if (wave.currentWaveNumber.Value >= waveDatas.Length)
                        continue;

                    wave.currentWaveEnemies.Value.AddRange(waveDatas[wave.currentWaveNumber.Value].EntityConfigs);
                    wave.ReplaceCurrentWaveNumber(wave.currentWaveNumber.Value += 1);
                    wave.isWaveInProgress = true;
                }

                waveRequst.isDestructed = true;
            }
        }
    }
}
