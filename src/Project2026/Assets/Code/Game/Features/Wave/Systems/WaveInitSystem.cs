using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Wave.Systems
{
    public class WaveInitSystem : IInitializeSystem
    {
        private readonly IGroup<GameEntity> _sessions;

        private readonly List<GameEntity> _sessionBuffer = new(2);

        public WaveInitSystem()
        {
            _sessions = Contexts.sharedInstance.game.GetGroup(GameMatcher
                .AllOf(GameMatcher.GameSession, GameMatcher.ActiveLevel));
        }

        public void Initialize()
        {
            var sessions = _sessions.GetEntities(_sessionBuffer);

            if (sessions.Count == 0)
                return;

            var waves = sessions[0].activeLevel.Value.Waves;
            if (waves == null)
                return;

            var entity = CreateGameEntity.Empty();

            entity.AddCurrentWaveNumber(0);
            entity.AddWaveEnemiesAlive(0);
            entity.AddCooldown(0);
            entity.AddCurrentWaveEnemies(new());
            entity.AddWaveSchedule(waves);
            entity.isWaveInProgress = false;
        }
    }
}
