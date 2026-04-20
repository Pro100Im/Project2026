using Code.Game.Common.Entity;
using Code.Game.StaticData.Configs;
using Entitas;
using System.Collections.Generic;
using Assets.Code.Game.StaticData.Property;

namespace Code.Game.Features.Wave.Systems
{
    public class WaveProgressSystem : IExecuteSystem
    {
        private readonly WavesConfig _wavesConfig;
        private readonly IGroup<GameEntity> _waves;

        private readonly List<GameEntity> _buffer = new(5);

        public WaveProgressSystem(GameContext gameContext, WavesConfig wavesConfig)
        {
            _wavesConfig = wavesConfig;

            _waves = gameContext.GetGroup(GameMatcher
              .AllOf(
              GameMatcher.CurrentWaveNumber,
              GameMatcher.WaveEnemiesAlive,
              GameMatcher.Cooldown,
              GameMatcher.CurrentWaveEnemies,
              GameMatcher.WaveInProgress));
        } 

        public void Execute()
        {
            foreach (var wave in _waves.GetEntities(_buffer))
            {
                if (wave.currentWaveEnemies.Value.Count == 0 && wave.waveEnemiesAlive.Value == 0)
                {
                    wave.isWaveInProgress = false;
                    wave.ReplaceCooldown(0);
                }
                else if (wave.currentWaveEnemies.Value.Count > 0 && wave.cooldown.Value <= 0)
                {
                    var entityConfig = wave.currentWaveEnemies.Value[0];
                    var entity = CreateGameEntity.Empty();
                    var newCooldown = _wavesConfig.WaveDatas[wave.currentWaveNumber.Value-1].Cooldown;
                    var unitSize = entityConfig.GetProperty<UnitSizeProperty>().Size;

                    entity.AddEntityConfig(entityConfig);
                    entity.AddUnitSize(unitSize);
                    entity.isSpawnRequsted = true;
                    entity.isEnemy = true;

                    wave.currentWaveEnemies.Value.Remove(entityConfig);
                    wave.ReplaceCooldown(newCooldown);
                }
            }
        }
    }
}