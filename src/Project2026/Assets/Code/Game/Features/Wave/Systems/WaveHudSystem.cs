using Code.Meta.Features.Game;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Wave.Systems
{
    public class WaveHudSystem : IExecuteSystem
    {
        private readonly GameScreen _gameScreen;
        private readonly IGroup<GameEntity> _waves;
        private readonly IGroup<GameEntity> _pendingEnemySpawns;
        private readonly IGroup<GameEntity> _livingEnemyUnits;

        private readonly List<GameEntity> _waveBuffer = new(2);

        public WaveHudSystem(GameScreen gameScreen)
        {
            var gameContext = Contexts.sharedInstance.game;

            _gameScreen = gameScreen;

            _waves = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.CurrentWaveNumber,
                    GameMatcher.CurrentWaveEnemies,
                    GameMatcher.WaveSchedule));

            _pendingEnemySpawns = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Enemy,
                    GameMatcher.SpawnRequsted));

            _livingEnemyUnits = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Enemy,
                    GameMatcher.Unit)
                .NoneOf(GameMatcher.Dead));
        }

        public void Execute()
        {
            var waves = _waves.GetEntities(_waveBuffer);

            if (waves.Count == 0)
                return;

            var wave = waves[0];
            var total = wave.waveSchedule.Value.WaveDatas.Length;
            var current = wave.currentWaveNumber.Value;
            var queued = wave.currentWaveEnemies.Value.Count;
            var remaining = queued + _pendingEnemySpawns.count + _livingEnemyUnits.count;
            var canCall = current < total;
            var displayWave = current;

            if (!wave.isWaveInProgress && remaining == 0 && canCall)
                displayWave = current + 1;

            if (displayWave < 1)
                displayWave = 1;

            if (total < 1)
                total = 1;

            _gameScreen.SetWaveHud(displayWave, total, remaining, canCall);
        }
    }
}
