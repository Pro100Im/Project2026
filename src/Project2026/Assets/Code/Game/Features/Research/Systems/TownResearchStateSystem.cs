using Code.Game.Features.Town;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Research.Systems
{
    public class TownResearchStateSystem : IExecuteSystem
    {
        private readonly IGroup<TownEntity> _nodes;
        private readonly IGroup<TownEntity> _exchequer;
        private readonly IGroup<TownEntity> _buildings;

        private readonly List<TownEntity> _nodesBuffer = new(32);
        private readonly List<TownEntity> _buildingsBuffer = new(16);

        public TownResearchStateSystem()
        {
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
                    TownMatcher.TownResources,
                    TownMatcher.TownKnowledge));

            _buildings = town.GetGroup(TownMatcher
                .AllOf(
                    TownMatcher.TownBuilding,
                    TownMatcher.TownBuildingType,
                    TownMatcher.TownBuildingLevel));
        }

        public void Execute()
        {
            var exchequer = _exchequer.GetSingleEntity();

            if (exchequer == null)
                return;

            var knowledge = exchequer.townKnowledge.Value;
            var nodes = _nodes.GetEntities(_nodesBuffer);

            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var state = ResolveState(node, nodes, knowledge);

                if (node.hasResearchNodeState && node.researchNodeState.Value == state)
                    continue;

                if (node.hasResearchNodeState)
                    node.ReplaceResearchNodeState(state);
                else
                    node.AddResearchNodeState(state);
            }
        }

        private ResearchNodeState ResolveState(TownEntity node, List<TownEntity> nodes, int knowledge)
        {
            if (node.researchLevel.Value >= node.researchMaxLevel.Value)
                return ResearchNodeState.Owned;

            if (!ParentOwned(node, nodes) || !BuildingReady(node))
                return ResearchNodeState.Locked;

            return knowledge < node.researchCost.Value
                ? ResearchNodeState.Unaffordable
                : ResearchNodeState.Available;
        }

        private static bool ParentOwned(TownEntity node, List<TownEntity> nodes)
        {
            if (!node.hasResearchParent)
                return true;

            var parentId = node.researchParent.Value;

            for (var i = 0; i < nodes.Count; i++)
            {
                var candidate = nodes[i];

                if (candidate.researchId.Value != parentId)
                    continue;

                return candidate.researchLevel.Value > 0;
            }

            return false;
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
