using Code.Game.StaticData.Configs;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Research.Systems
{
    public class TownResearchLayoutSystem : IInitializeSystem
    {
        private readonly ResearchCatalog _catalog;
        private readonly IGroup<TownEntity> _nodes;
        private readonly List<TownEntity> _buffer = new(32);

        public TownResearchLayoutSystem(ResearchCatalog catalog)
        {
            _catalog = catalog;
            _nodes = Contexts.sharedInstance.town.GetGroup(TownMatcher
                .AllOf(
                    TownMatcher.ResearchNode,
                    TownMatcher.ResearchId,
                    TownMatcher.ResearchDirection));
        }

        public void Initialize()
        {
            var spacing = _catalog != null ? _catalog.NodeSpacing : 72f;

            if (spacing <= 0f)
                spacing = 72f;

            var nodes = _nodes.GetEntities(_buffer);
            var count = nodes.Count;
            var placed = new bool[count];
            var positions = new Vector2[count];
            var guard = 0;

            while (guard < count)
            {
                var progress = false;

                for (var i = 0; i < count; i++)
                {
                    if (placed[i])
                        continue;

                    var parentIndex = ParentIndex(nodes, i);

                    if (parentIndex >= 0 && !placed[parentIndex])
                        continue;

                    var origin = parentIndex >= 0 ? positions[parentIndex] : Vector2.zero;
                    var direction = nodes[i].researchDirection.Value;
                    var step = 1;

                    for (var j = 0; j < count; j++)
                    {
                        if (!placed[j])
                            continue;

                        if (ParentIndex(nodes, j) != parentIndex)
                            continue;

                        if (nodes[j].researchDirection.Value != direction)
                            continue;

                        step++;
                    }

                    positions[i] = origin + DirectionStep(direction) * (spacing * step);
                    placed[i] = true;
                    progress = true;
                }

                if (!progress)
                    break;

                guard++;
            }

            for (var i = 0; i < count; i++)
            {
                var node = nodes[i];

                if (node.hasResearchNodePosition)
                    node.ReplaceResearchNodePosition(positions[i]);
                else
                    node.AddResearchNodePosition(positions[i]);
            }
        }

        private static int ParentIndex(List<TownEntity> nodes, int index)
        {
            var node = nodes[index];

            if (!node.hasResearchParent)
                return -1;

            var parentId = node.researchParent.Value;

            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].researchId.Value == parentId)
                    return i;
            }

            return -1;
        }

        private static Vector2 DirectionStep(ResearchLinkDirection direction)
        {
            var angle = direction switch
            {
                ResearchLinkDirection.East => 0f,
                ResearchLinkDirection.NorthEast => 45f,
                ResearchLinkDirection.North => 90f,
                ResearchLinkDirection.NorthWest => 135f,
                ResearchLinkDirection.West => 180f,
                ResearchLinkDirection.SouthWest => 225f,
                ResearchLinkDirection.South => 270f,
                ResearchLinkDirection.SouthEast => 315f,
                _ => 0f
            };
            var radians = angle * Mathf.Deg2Rad;

            return new Vector2(Mathf.Cos(radians), -Mathf.Sin(radians));
        }
    }
}
