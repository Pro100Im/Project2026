using Code.Game.Common.Entity;
using Code.Game.StaticData.Configs;
using Code.Infrastructure.Persistence;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Research.Systems
{
    public class GameResearchSpawnSystem : IInitializeSystem
    {
        private readonly ResearchCatalog _catalog;
        private readonly IUserProgressService _progress;
        private readonly IGroup<GameEntity> _nodes;
        private readonly List<GameEntity> _buffer = new(4);

        public GameResearchSpawnSystem(ResearchCatalog catalog, IUserProgressService progress)
        {
            _catalog = catalog;
            _progress = progress;
            _nodes = Contexts.sharedInstance.game.GetGroup(GameMatcher.ResearchNode);
        }

        public void Initialize()
        {
            if (_catalog == null || _catalog.Nodes == null || _nodes.GetEntities(_buffer).Count > 0)
                return;

            var definitions = _catalog.Nodes;

            for (var i = 0; i < definitions.Length; i++)
            {
                var definition = definitions[i];

                if (definition == null)
                    continue;

                definition.ApplyGame(CreateGameEntity.Empty(), definition.ResolveLevel(_progress));
            }
        }
    }
}
