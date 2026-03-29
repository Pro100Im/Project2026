using Code.Common.Entity;
using Code.Common.Time;
using Code.Game.StaticData.Configs;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Wave.Systems
{
    public class WaveSystem : IInitializeSystem, IExecuteSystem
    {
        private readonly WavesConfig _wavesConfig;
        private readonly ITimeService _timeService;
        private readonly IGroup<GameEntity> _waves;
        private readonly IGroup<GameEntity> _waveRequsts;

        private readonly List<GameEntity> _buffer = new(5);

        public WaveSystem(GameContext gameContext, WavesConfig wavesConfig, ITimeService timeService)
        {
            _wavesConfig = wavesConfig;
            _timeService = timeService;

            _waves = gameContext.GetGroup(GameMatcher
              .AllOf(
              GameMatcher.CurrentWaveNumber,
              GameMatcher.WaveEnemiesAlive,
              GameMatcher.Cooldown,
              GameMatcher.CurrentWaveEnemies));

            _waveRequsts = gameContext.GetGroup(GameMatcher.WaveStartRequsted);
        } 

        public void Initialize()
        {
            var entity = CreateGameEntity.Empty();

            entity.AddCurrentWaveNumber(0);
            entity.AddWaveEnemiesAlive(0);
            entity.AddCooldown(0);
            entity.AddCurrentWaveEnemies(new());
            entity.isWaveInProgress = false;
        }

        public void Execute()
        {
            foreach(var waveRequst in _waveRequsts.GetEntities(_buffer))
            {
                foreach (var wave in _waves)
                {
                    Debug.Log("Start wave " + wave.currentWaveNumber.Value);
                    // To Do and separate
                    wave.currentWaveEnemies.Value.AddRange(_wavesConfig.WaveDatas[wave.currentWaveNumber.Value].EntityConfigs);
                    wave.isWaveInProgress = true;
                }

                waveRequst.Destroy();
            }

            // To do and select spawn pos system
            foreach (var wave in _waves)
            {
                if (wave.currentWaveEnemies.Value.Count == 0 && wave.waveEnemiesAlive.Value == 0)
                {
                    wave.isWaveInProgress = false;
                    wave.ReplaceCurrentWaveNumber(wave.currentWaveNumber.Value++);
                    wave.ReplaceCooldown(0);
                }
                else if (wave.cooldown.Value > 0)
                {
                    var newValue = wave.cooldown.Value - _timeService.DeltaTime;

                    wave.ReplaceCooldown(newValue);
                }
                else if (wave.currentWaveEnemies.Value.Count > 0)
                {
                    var entityConfig = wave.currentWaveEnemies.Value[0];
                    var entity = CreateGameEntity.Empty();
                    var newCooldown = _wavesConfig.WaveDatas[wave.currentWaveNumber.Value].Ñooldown;

                    entity.AddEntityConfig(entityConfig);
                    entity.isSpawnRequsted = true;
                    entity.isEnemy = true;

                    wave.currentWaveEnemies.Value.Remove(entityConfig);
                    wave.ReplaceCooldown(newCooldown);
                }
            }
        }
    }
}