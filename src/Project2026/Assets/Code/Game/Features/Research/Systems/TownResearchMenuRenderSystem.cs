using Code.Meta.Features.Town;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Research.Systems
{
    public class TownResearchMenuRenderSystem : ReactiveSystem<TownEntity>
    {
        private readonly IGroup<TownEntity> _menus;
        private readonly IGroup<TownEntity> _nodes;
        private readonly List<TownEntity> _buffer = new(32);
        private readonly List<ResearchLink> _links = new(32);

        public TownResearchMenuRenderSystem() : base(Contexts.sharedInstance.town)
        {
            var town = Contexts.sharedInstance.town;
            _menus = town.GetGroup(TownMatcher.TownResearchMenu);
            _nodes = town.GetGroup(TownMatcher
                .AllOf(
                    TownMatcher.ResearchNode,
                    TownMatcher.ResearchId,
                    TownMatcher.ResearchTitle,
                    TownMatcher.ResearchDescription,
                    TownMatcher.ResearchCost,
                    TownMatcher.ResearchNodePosition,
                    TownMatcher.ResearchNodeState));
        }

        protected override ICollector<TownEntity> GetTrigger(IContext<TownEntity> context) =>
            context.CreateCollector(
                TownMatcher.TownResearchMenuOpened.Added(),
                TownMatcher.ResearchNodeState.Added(),
                TownMatcher.ResearchNodePosition.Added());

        protected override bool Filter(TownEntity entity) => entity.isEnabled;

        protected override void Execute(List<TownEntity> entities)
        {
            var menu = _menus.GetSingleEntity();

            if (menu == null || !menu.isTownResearchMenuOpened || !menu.hasTownResearchMenu)
                return;

            var view = menu.townResearchMenu.Value;
            var nodes = _nodes.GetEntities(_buffer);
            _links.Clear();

            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var position = node.researchNodePosition.Value;
                var icon = node.hasResearchIcon ? node.researchIcon.Value : null;

                view.SetNode(
                    node.researchId.Value,
                    node.researchTitle.Value,
                    node.researchDescription.Value,
                    icon,
                    node.researchCost.Value,
                    node.researchNodeState.Value,
                    position);

                _links.Add(new ResearchLink(ParentPosition(node, nodes), position, node.researchNodeState.Value == ResearchNodeState.Owned));
            }

            view.SetLinks(_links);
        }

        private static Vector2 ParentPosition(TownEntity node, List<TownEntity> nodes)
        {
            if (!node.hasResearchParent)
                return Vector2.zero;

            var parentId = node.researchParent.Value;

            for (var i = 0; i < nodes.Count; i++)
            {
                var candidate = nodes[i];

                if (!candidate.hasResearchId || candidate.researchId.Value != parentId || !candidate.hasResearchNodePosition)
                    continue;

                return candidate.researchNodePosition.Value;
            }

            return Vector2.zero;
        }
    }
}
