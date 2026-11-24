using Code.Game.Common.Entity;
using Code.Game.StaticData.Configs;
using Code.Infrastructure.Persistence;
using Entitas;

namespace Code.Meta.Features.LevelMap.Systems
{
    /// <summary>
    /// Spawns one meta entity per level map node from <see cref="LevelCatalog"/>.
    /// Re-spawns after the meta context was reset by another world.
    /// </summary>
    public class LevelNodeSpawnSystem : IInitializeSystem, IExecuteSystem
    {
        private readonly LevelCatalog _catalog;
        private readonly IUserProgressService _progress;
        private readonly IGroup<MetaEntity> _nodes;

        public LevelNodeSpawnSystem(LevelCatalog catalog, IUserProgressService progress)
        {
            _catalog = catalog;
            _progress = progress;
            _nodes = Contexts.sharedInstance.meta.GetGroup(MetaMatcher.LevelNode);
        }

        public void Initialize() => Execute();

        public void Execute()
        {
            if (_catalog == null || _nodes.count > 0)
                return;

            for (var i = 0; i < _catalog.Count; i++)
            {
                var definition = _catalog.Get(i);

                if (definition == null || string.IsNullOrEmpty(definition.Id))
                    continue;

                definition.ApplyMeta(CreateMetaEntity.Empty(), _progress);
            }
        }
    }
}
