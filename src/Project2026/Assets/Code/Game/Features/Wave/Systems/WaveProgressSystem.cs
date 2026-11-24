using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;
using Assets.Code.Game.StaticData.Property;

namespace Code.Game.Features.Wave.Systems
{
    public class WaveProgressSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _waves;

        private readonly List<GameEntity> _buffer = new(16);

        public WaveProgressSystem()
        {
            _waves = Contexts.sharedInstance.game.GetGroup(GameMatcher
              .AllOf(
              GameMatcher.CurrentWaveNumber,
              GameMatcher.WaveEnemiesAlive,
              GameMatcher.Cooldown,
              GameMatcher.CurrentWaveEnemies,
              GameMatcher.WaveSchedule,
              GameMatcher.WaveInProgress));
        }

        public void Execute()
        {
            var waves = _waves.GetEntities(_buffer);

            for (var i = 0; i < waves.Count; i++)
            {
                var wave = waves[i];

                if (wave.currentWaveEnemies.Value.Count == 0 && wave.waveEnemiesAlive.Value == 0)
                {
                    wave.isWaveInProgress = false;
                    wave.ReplaceCooldown(0);
                }
                else if (wave.currentWaveEnemies.Value.Count > 0 && wave.cooldown.Value <= 0)
                {
                    var entityConfig = wave.currentWaveEnemies.Value[0];
                    var entity = CreateGameEntity.Empty();
                    var newCooldown = wave.waveSchedule.Value.WaveDatas[wave.currentWaveNumber.Value - 1].Cooldown;
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
