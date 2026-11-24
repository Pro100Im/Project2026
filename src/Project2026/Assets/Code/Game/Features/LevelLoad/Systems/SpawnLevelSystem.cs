using Code.Infrastructure.Factories;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.LevelLoad.Systems
{
    public class SpawnLevelSystem : IInitializeSystem
    {
        private readonly ILevelFactory _levelFactory;
        private readonly IGroup<GameEntity> _requests;

        private readonly List<GameEntity> _buffer = new(2);

        public SpawnLevelSystem(ILevelFactory levelFactory)
        {
            _levelFactory = levelFactory;
            _requests = Contexts.sharedInstance.game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.GameSession,
                    GameMatcher.ActiveLevel,
                    GameMatcher.LevelSpawnRequested)
                .NoneOf(GameMatcher.LevelLoaded));
        }

        public void Initialize()
        {
            var requests = _requests.GetEntities(_buffer);

            for (var i = 0; i < requests.Count; i++)
            {
                var session = requests[i];
                var prefab = session.activeLevel.Value.LevelPrefab;

                _levelFactory.Create(prefab);

                session.isLevelSpawnRequested = false;
                session.isLevelLoaded = true;
            }
        }
    }
}
