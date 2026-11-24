using Code.Game.StaticData.Configs;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.LevelLoad.Systems
{
    public class RequestSelectedLevelSystem : IInitializeSystem
    {
        private readonly LevelCatalog _catalog;
        private readonly IGroup<GameEntity> _sessions;

        private readonly List<GameEntity> _sessionBuffer = new(2);

        public RequestSelectedLevelSystem(LevelCatalog catalog)
        {
            _catalog = catalog;
            _sessions = Contexts.sharedInstance.game.GetGroup(GameMatcher.GameSession);
        }

        public void Initialize()
        {
            var sessions = _sessions.GetEntities(_sessionBuffer);

            if (sessions.Count == 0)
                return;

            var node = _catalog.ResolveNode(ResolveSelectedNodeId());

            if (node == null || node.Level == null)
                return;

            for (var i = 0; i < sessions.Count; i++)
            {
                var session = sessions[i];

                if (session.hasActiveLevel)
                    session.ReplaceActiveLevel(node.Level);
                else
                    session.AddActiveLevel(node.Level);

                if (session.hasActiveLevelNode)
                    session.ReplaceActiveLevelNode(node.Id);
                else
                    session.AddActiveLevelNode(node.Id);

                session.isLevelSpawnRequested = true;
                session.isLevelLoaded = false;
            }
        }

        private static string ResolveSelectedNodeId()
        {
            var input = Contexts.sharedInstance.input;

            if (!input.hasSelectedLevel)
                return null;

            return input.selectedLevel.Id;
        }
    }
}
