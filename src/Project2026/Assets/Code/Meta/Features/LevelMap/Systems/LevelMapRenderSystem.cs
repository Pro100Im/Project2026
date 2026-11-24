using Code.Game.StaticData.Configs;
using Code.Meta.UI.MainMenu;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Meta.Features.LevelMap.Systems
{
    /// <summary>
    /// Pushes the map to the view when the open map is dirty. Locked nodes stay hidden, a road is shown only between revealed nodes.
    /// </summary>
    public class LevelMapRenderSystem : IExecuteSystem
    {
        private readonly IGroup<MetaEntity> _menus;
        private readonly IGroup<MetaEntity> _nodes;
        private readonly List<MetaEntity> _buffer = new(16);
        private readonly List<LevelMapLink> _links = new(16);

        public LevelMapRenderSystem()
        {
            var meta = Contexts.sharedInstance.meta;
            _menus = meta.GetGroup(MetaMatcher.LevelMapMenu);
            _nodes = meta.GetGroup(MetaMatcher
                .AllOf(
                    MetaMatcher.LevelNode,
                    MetaMatcher.LevelNodeId,
                    MetaMatcher.LevelNodeRequires,
                    MetaMatcher.LevelNodeTitle,
                    MetaMatcher.LevelNodePosition,
                    MetaMatcher.LevelNodeStars,
                    MetaMatcher.LevelNodeState));
        }

        public void Execute()
        {
            var menu = _menus.GetSingleEntity();

            if (menu == null || !menu.isLevelMapMenuOpened || !menu.isLevelMapDirty)
                return;

            menu.isLevelMapDirty = false;

            var view = menu.levelMapMenu.Value;
            view.SetMap(
                menu.hasLevelMapSize ? menu.levelMapSize.Value : Vector2.zero,
                menu.hasLevelMapBackground ? menu.levelMapBackground.Value : null);

            var nodes = _nodes.GetEntities(_buffer);
            var stars = 0;
            _links.Clear();

            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var state = node.levelNodeState.Value;
                stars += node.levelNodeStars.Value;

                view.SetNode(
                    node.levelNodeId.Value,
                    node.levelNodeTitle.Value,
                    state,
                    node.levelNodeStars.Value,
                    node.levelNodePosition.Value);

                if (state == LevelNodeState.Locked)
                    continue;

                AddRoads(node, nodes);
            }

            view.SetLinks(_links);
            view.SetStars(stars, nodes.Count * LevelConfig.MaxStars);
        }

        private void AddRoads(MetaEntity node, List<MetaEntity> nodes)
        {
            var requires = node.levelNodeRequires.Value;

            if (requires == null)
                return;

            for (var r = 0; r < requires.Length; r++)
            {
                var parent = Find(requires[r], nodes);

                if (parent == null || parent.levelNodeState.Value == LevelNodeState.Locked)
                    continue;

                _links.Add(new LevelMapLink(
                    parent.levelNodePosition.Value,
                    node.levelNodePosition.Value,
                    parent.isLevelNodeCompleted));
            }
        }

        private static MetaEntity Find(string id, List<MetaEntity> nodes)
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].levelNodeId.Value == id)
                    return nodes[i];
            }

            return null;
        }
    }
}
