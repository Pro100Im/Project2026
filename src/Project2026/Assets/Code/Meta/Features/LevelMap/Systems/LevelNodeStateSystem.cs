using Entitas;
using System.Collections.Generic;

namespace Code.Meta.Features.LevelMap.Systems
{
    /// <summary>
    /// Completed node stays completed (replayable). A node is available when it has no
    /// requirements, or when any (or all, with RequireAll) of the required nodes are completed.
    /// </summary>
    public class LevelNodeStateSystem : IExecuteSystem
    {
        private readonly IGroup<MetaEntity> _menus;
        private readonly IGroup<MetaEntity> _nodes;
        private readonly List<MetaEntity> _buffer = new(16);

        public LevelNodeStateSystem()
        {
            _menus = Contexts.sharedInstance.meta.GetGroup(MetaMatcher.LevelMapMenu);
            _nodes = Contexts.sharedInstance.meta.GetGroup(MetaMatcher
                .AllOf(
                    MetaMatcher.LevelNode,
                    MetaMatcher.LevelNodeId,
                    MetaMatcher.LevelNodeRequires));
        }

        public void Execute()
        {
            var nodes = _nodes.GetEntities(_buffer);
            var changed = false;

            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var state = ResolveState(node, nodes);

                if (node.hasLevelNodeState && node.levelNodeState.Value == state)
                    continue;

                node.ReplaceLevelNodeState(state);
                changed = true;
            }

            var menu = _menus.GetSingleEntity();

            if (changed && menu != null)
                menu.isLevelMapDirty = true;
        }

        private static LevelNodeState ResolveState(MetaEntity node, List<MetaEntity> nodes)
        {
            if (node.isLevelNodeCompleted)
                return LevelNodeState.Completed;

            return RequirementsMet(node, nodes) ? LevelNodeState.Available : LevelNodeState.Locked;
        }

        private static bool RequirementsMet(MetaEntity node, List<MetaEntity> nodes)
        {
            var requires = node.levelNodeRequires.Value;

            if (requires == null || requires.Length == 0)
                return true;

            var requireAll = node.isLevelNodeRequireAll;

            for (var i = 0; i < requires.Length; i++)
            {
                var completed = IsCompleted(requires[i], nodes);

                if (requireAll && !completed)
                    return false;

                if (!requireAll && completed)
                    return true;
            }

            return requireAll;
        }

        private static bool IsCompleted(string id, List<MetaEntity> nodes)
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                var candidate = nodes[i];

                if (candidate.levelNodeId.Value == id)
                    return candidate.isLevelNodeCompleted;
            }

            return false;
        }
    }
}
