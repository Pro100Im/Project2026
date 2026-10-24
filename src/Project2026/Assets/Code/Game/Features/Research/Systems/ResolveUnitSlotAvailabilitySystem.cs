using Code.Infrastructure.Persistence;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Research.Systems
{
    public class ResolveUnitSlotAvailabilitySystem : IExecuteSystem
    {
        private readonly IUserProgressService _progress;
        private readonly IGroup<GameEntity> _nodes;
        private readonly IGroup<GameEntity> _slots;
        private readonly List<GameEntity> _nodesBuffer = new(32);
        private readonly List<GameEntity> _slotsBuffer = new(4);

        public ResolveUnitSlotAvailabilitySystem(IUserProgressService progress)
        {
            _progress = progress;

            var game = Contexts.sharedInstance.game;
            _nodes = game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.ResearchNode,
                    GameMatcher.ResearchLevel,
                    GameMatcher.ResearchUnitSlotUnlock));
            _slots = game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.UnitSlot,
                    GameMatcher.UnitKind));
        }

        public void Execute()
        {
            var nodes = _nodes.GetEntities(_nodesBuffer);
            var slots = _slots.GetEntities(_slotsBuffer);

            for (var i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                var available = IsAvailable(slot.unitKind.Value, nodes);

                if (available == slot.isUnitSlotAvailable)
                    continue;

                slot.isUnitSlotAvailable = available;
            }
        }

        private bool IsAvailable(UnitKind kind, List<GameEntity> nodes)
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];

                if (node.researchLevel.Value <= 0 || node.researchUnitSlotUnlock.Value != kind)
                    continue;

                if (!BuildingReady(node))
                    continue;

                return true;
            }

            return false;
        }

        private bool BuildingReady(GameEntity node)
        {
            if (!node.hasResearchRequiredBuilding)
                return true;

            var requiredLevel = node.hasResearchRequiredBuildingLevel
                ? node.researchRequiredBuildingLevel.Value
                : 1;

            return _progress.TryGetBuildingLevel(node.researchRequiredBuilding.Value, out var level)
                && level >= requiredLevel;
        }
    }
}
