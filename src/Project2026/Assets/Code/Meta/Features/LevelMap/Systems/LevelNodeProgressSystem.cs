using Code.Infrastructure.Persistence;
using Entitas;
using System.Collections.Generic;

namespace Code.Meta.Features.LevelMap.Systems
{
    /// <summary>
    /// Syncs node completion, stars and the last seen state from the save while the open map is dirty (e.g. right after opening).
    /// </summary>
    public class LevelNodeProgressSystem : IExecuteSystem
    {
        private readonly IUserProgressService _progress;
        private readonly IGroup<MetaEntity> _menus;
        private readonly IGroup<MetaEntity> _nodes;
        private readonly List<MetaEntity> _buffer = new(16);

        public LevelNodeProgressSystem(IUserProgressService progress)
        {
            _progress = progress;

            var meta = Contexts.sharedInstance.meta;
            _menus = meta.GetGroup(MetaMatcher.AllOf(MetaMatcher.LevelMapMenu, MetaMatcher.LevelMapMenuOpened, MetaMatcher.LevelMapDirty));
            _nodes = meta.GetGroup(MetaMatcher.AllOf(MetaMatcher.LevelNode, MetaMatcher.LevelNodeId));
        }

        public void Execute()
        {
            if (_menus.count == 0)
                return;

            var nodes = _nodes.GetEntities(_buffer);

            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                node.isLevelNodeCompleted = _progress.TryGetLevelStars(node.levelNodeId.Value, out var stars);

                if (!node.hasLevelNodeStars || node.levelNodeStars.Value != stars)
                    node.ReplaceLevelNodeStars(stars);

                // Never seen = was hidden under the clouds.
                _progress.TryGetSeenLevelState(node.levelNodeId.Value, out var seen);

                if (!node.hasLevelNodeSeenState || node.levelNodeSeenState.Value != seen)
                    node.ReplaceLevelNodeSeenState(seen);
            }
        }
    }
}
