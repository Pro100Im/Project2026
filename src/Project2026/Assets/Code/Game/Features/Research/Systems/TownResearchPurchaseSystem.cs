using Code.Game.Features.Town;
using Code.Infrastructure.Persistence;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Research.Systems
{
    public class TownResearchPurchaseSystem : ReactiveSystem<InputEntity>
    {
        private readonly IUserProgressService _progress;

        private readonly IGroup<TownEntity> _nodes;
        private readonly IGroup<TownEntity> _exchequer;
        private readonly IGroup<TownEntity> _buildings;

        private readonly List<TownEntity> _nodesBuffer = new(32);
        private readonly List<TownEntity> _buildingsBuffer = new(16);

        public TownResearchPurchaseSystem(IUserProgressService progress) : base(Contexts.sharedInstance.input)
        {
            _progress = progress;

            var town = Contexts.sharedInstance.town;
            _nodes = town.GetGroup(TownMatcher
                .AllOf(
                    TownMatcher.ResearchNode,
                    TownMatcher.ResearchId,
                    TownMatcher.ResearchLevel,
                    TownMatcher.ResearchMaxLevel,
                    TownMatcher.ResearchCost));
            _exchequer = town.GetGroup(TownMatcher
                .AllOf(
                    TownMatcher.TownExchequer,
                    TownMatcher.TownResources,
                    TownMatcher.TownGold,
                    TownMatcher.TownStone,
                    TownMatcher.TownCrystal,
                    TownMatcher.TownWood,
                    TownMatcher.TownKnowledge));
            _buildings = town.GetGroup(TownMatcher
                .AllOf(
                    TownMatcher.TownBuilding,
                    TownMatcher.TownBuildingType,
                    TownMatcher.TownBuildingLevel));
        }

        protected override ICollector<InputEntity> GetTrigger(IContext<InputEntity> context) =>
            context.CreateCollector(InputMatcher.TownResearchPurchaseRequested.Added());

        protected override bool Filter(InputEntity entity) => entity.hasTownResearchPurchaseRequested;

        protected override void Execute(List<InputEntity> entities)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var request = entities[i];
                TryPurchase(request.townResearchPurchaseRequested.Value);
                request.RemoveTownResearchPurchaseRequested();
                request.isDestructed = true;
            }
        }

        private void TryPurchase(ResearchId id)
        {
            var exchequer = _exchequer.GetSingleEntity();

            if (exchequer == null)
                return;

            var nodes = _nodes.GetEntities(_nodesBuffer);
            var node = Find(nodes, id);

            if (node == null)
                return;

            if (node.researchLevel.Value >= node.researchMaxLevel.Value)
                return;

            if (!ParentOwned(node, nodes) || !BuildingReady(node))
                return;

            var cost = node.researchCost.Value;
            var knowledge = exchequer.townKnowledge.Value;

            if (knowledge < cost)
                return;

            var nextLevel = node.researchLevel.Value + 1;
            var nextKnowledge = knowledge - cost;

            node.ReplaceResearchLevel(nextLevel);
            exchequer.ReplaceTownKnowledge(nextKnowledge);

            if (exchequer.townExchequer.Value != null)
                exchequer.townExchequer.Value.SetKnowledge(nextKnowledge);

            _progress.SetResearchLevel(id, nextLevel);
            _progress.SetTownResources(
                exchequer.townGold.Value,
                exchequer.townStone.Value,
                exchequer.townCrystal.Value,
                exchequer.townWood.Value,
                nextKnowledge);
        }

        private static TownEntity Find(List<TownEntity> nodes, ResearchId id)
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].researchId.Value == id)
                    return nodes[i];
            }

            return null;
        }

        private static bool ParentOwned(TownEntity node, List<TownEntity> nodes)
        {
            if (!node.hasResearchParent)
                return true;

            var parent = Find(nodes, node.researchParent.Value);
            return parent != null && parent.researchLevel.Value > 0;
        }

        private bool BuildingReady(TownEntity node)
        {
            if (!node.hasResearchRequiredBuilding)
                return true;

            var requiredLevel = node.hasResearchRequiredBuildingLevel
                ? node.researchRequiredBuildingLevel.Value
                : 1;

            var buildings = _buildings.GetEntities(_buildingsBuffer);
            var type = node.researchRequiredBuilding.Value;

            for (var i = 0; i < buildings.Count; i++)
            {
                var building = buildings[i];

                if (building.townBuildingType.Value != type)
                    continue;

                return building.townBuildingLevel.Value >= requiredLevel;
            }

            return false;
        }
    }
}
