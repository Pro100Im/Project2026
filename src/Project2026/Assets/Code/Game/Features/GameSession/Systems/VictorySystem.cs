using Code.Game.Common.Entity;
using Code.Game.StaticData.Configs;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.GameSession.Systems
{
    public class VictorySystem : IExecuteSystem
    {
        private readonly WavesConfig _wavesConfig;
        private readonly IGroup<GameEntity> _sessions;
        private readonly IGroup<GameEntity> _waves;
        private readonly IGroup<GameEntity> _pendingEnemySpawns;
        private readonly IGroup<GameEntity> _livingEnemyUnits;
        private readonly IGroup<GameEntity> _castles;

        private readonly List<GameEntity> _sessionBuffer = new(2);
        private readonly List<GameEntity> _waveBuffer = new(2);
        private readonly List<GameEntity> _castleBuffer = new(2);

        public VictorySystem(WavesConfig wavesConfig)
        {
            var gameContext = Contexts.sharedInstance.game;

            _wavesConfig = wavesConfig;

            _sessions = gameContext.GetGroup(GameMatcher
                .AllOf(GameMatcher.GameSession)
                .NoneOf(GameMatcher.Victory, GameMatcher.ForcedPause));

            _waves = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.CurrentWaveNumber,
                    GameMatcher.CurrentWaveEnemies));

            _pendingEnemySpawns = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Enemy,
                    GameMatcher.SpawnRequsted));

            _livingEnemyUnits = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.Enemy,
                    GameMatcher.Unit)
                .NoneOf(GameMatcher.Dead));

            _castles = gameContext.GetGroup(GameMatcher
                .AllOf(GameMatcher.PlayerCastle)
                .NoneOf(GameMatcher.Dead));
        }

        public void Execute()
        {
            var sessions = _sessions.GetEntities(_sessionBuffer);

            if (sessions.Count == 0)
                return;

            var waves = _waves.GetEntities(_waveBuffer);

            if (waves.Count == 0)
                return;

            var wave = waves[0];

            if (wave.currentWaveNumber.Value <= 0)
                return;

            if (wave.currentWaveNumber.Value < _wavesConfig.WaveDatas.Length)
                return;

            if (wave.currentWaveEnemies.Value.Count > 0)
                return;

            if (_pendingEnemySpawns.count > 0)
                return;

            if (_livingEnemyUnits.count > 0)
                return;

            var castles = _castles.GetEntities(_castleBuffer);

            if (castles.Count == 0)
                return;

            for (var i = 0; i < sessions.Count; i++)
                sessions[i].isVictory = true;

            var pauseRequest = CreateInputEntity.Empty();
            pauseRequest.isInput = true;
            pauseRequest.isForcedPauseRequested = true;
        }
    }
}
