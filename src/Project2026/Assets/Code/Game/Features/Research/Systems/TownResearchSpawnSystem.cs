using Code.Game.Common.Entity;
using Code.Game.StaticData.Configs;
using Code.Infrastructure.Persistence;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Research.Systems
{
    public class TownResearchSpawnSystem : IInitializeSystem
    {
        private readonly ResearchCatalog _catalog;
        private readonly IUserProgressService _progress;
        private readonly IGroup<TownEntity> _nodes;
        private readonly List<TownEntity> _buffer = new(4);
        private readonly List<ResearchNodeDefinition> _definitions = new(32);

        public TownResearchSpawnSystem(ResearchCatalog catalog, IUserProgressService progress)
        {
            _catalog = catalog;
            _progress = progress;
            _nodes = Contexts.sharedInstance.town.GetGroup(TownMatcher.ResearchNode);
        }

        public void Initialize()
        {
            if (_catalog == null || _nodes.GetEntities(_buffer).Count > 0)
                return;

            _catalog.Collect(_definitions);

            for (var i = 0; i < _definitions.Count; i++)
            {
                var definition = _definitions[i];

                if (definition == null)
                    continue;

                definition.ApplyTown(CreateTownEntity.Empty(), definition.ResolveLevel(_progress));
            }
        }
    }
}
