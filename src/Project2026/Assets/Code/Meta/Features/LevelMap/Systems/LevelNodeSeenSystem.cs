using Code.Infrastructure.Persistence;
using Entitas;
using System.Collections.Generic;

namespace Code.Meta.Features.LevelMap.Systems
{
    /// <summary>
    /// Runs after the render: once the open map has shown a node, its current state becomes the seen
    /// state and is saved, so the reveal / unlock animation for that change plays only once.
    /// Does not mark the map dirty: the view already shows the current state.
    /// </summary>
    public class LevelNodeSeenSystem : IExecuteSystem
    {
        private readonly IUserProgressService _progress;
        private readonly IGroup<MetaEntity> _menus;
        private readonly IGroup<MetaEntity> _nodes;
        private readonly List<MetaEntity> _buffer = new(16);

        public LevelNodeSeenSystem(IUserProgressService progress)
        {
            _progress = progress;

            var meta = Contexts.sharedInstance.meta;
            _menus = meta.GetGroup(MetaMatcher.AllOf(MetaMatcher.LevelMapMenu, MetaMatcher.LevelMapMenuOpened));
            _nodes = meta.GetGroup(MetaMatcher.AllOf(
                MetaMatcher.LevelNode,
                MetaMatcher.LevelNodeId,
                MetaMatcher.LevelNodeState,
                MetaMatcher.LevelNodeSeenState));
        }

        public void Execute()
        {
            var menu = _menus.GetSingleEntity();

            // Wait until the render has pushed the current state to the view.
            if (menu == null || menu.isLevelMapDirty)
                return;

            var nodes = _nodes.GetEntities(_buffer);

            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var state = node.levelNodeState.Value;

                if (node.levelNodeSeenState.Value == state)
                    continue;

                _progress.SetSeenLevelState(node.levelNodeId.Value, state);
                node.ReplaceLevelNodeSeenState(state);
            }
        }
    }
}
