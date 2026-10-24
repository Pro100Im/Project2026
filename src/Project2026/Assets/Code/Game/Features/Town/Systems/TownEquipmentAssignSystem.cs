using Code.Game.Features.Research;
using Code.Game.StaticData.Configs;
using Code.Infrastructure.Persistence;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Town.Systems
{
    public class TownEquipmentAssignSystem : ReactiveSystem<InputEntity>
    {
        private readonly IUserProgressService _progress;
        private readonly UnitRoster _roster;
        private readonly IGroup<TownEntity> _nodes;
        private readonly IGroup<TownEntity> _buildings;
        private readonly List<TownEntity> _nodesBuffer = new(32);
        private readonly List<TownEntity> _buildingsBuffer = new(16);

        public TownEquipmentAssignSystem(IUserProgressService progress, UnitRoster roster) : base(Contexts.sharedInstance.input)
        {
            _progress = progress;
            _roster = roster;

            var town = Contexts.sharedInstance.town;
            _nodes = town.GetGroup(TownMatcher
                .AllOf(
                    TownMatcher.ResearchNode,
                    TownMatcher.ResearchLevel,
                    TownMatcher.ResearchUnitSlotUnlock));
            _buildings = town.GetGroup(TownMatcher
                .AllOf(
                    TownMatcher.TownBuilding,
                    TownMatcher.TownBuildingType,
                    TownMatcher.TownBuildingLevel));
        }

        protected override ICollector<InputEntity> GetTrigger(IContext<InputEntity> context) =>
            context.CreateCollector(
                InputMatcher.TownEquipmentAssignRequested.Added(),
                InputMatcher.TownEquipmentClearRequested.Added());

        protected override bool Filter(InputEntity entity) =>
            entity.hasTownEquipmentAssignRequested || entity.hasTownEquipmentClearRequested;

        protected override void Execute(List<InputEntity> entities)
        {
            var nodes = _nodes.GetEntities(_nodesBuffer);
            var buildings = _buildings.GetEntities(_buildingsBuffer);

            for (var i = 0; i < entities.Count; i++)
            {
                var request = entities[i];

                if (request.hasTownEquipmentClearRequested)
                    Clear(request.townEquipmentClearRequested.Slot);
                else if (request.hasTownEquipmentAssignRequested)
                    Assign(request, nodes, buildings);

                if (request.hasTownEquipmentAssignRequested)
                    request.RemoveTownEquipmentAssignRequested();

                if (request.hasTownEquipmentClearRequested)
                    request.RemoveTownEquipmentClearRequested();

                request.isDestructed = true;
            }
        }

        private void Assign(InputEntity request, List<TownEntity> nodes, List<TownEntity> buildings)
        {
            if (_roster == null)
                return;

            var slot = request.townEquipmentAssignRequested.Slot;
            var kind = request.townEquipmentAssignRequested.Kind;

            if (slot < 0 || slot >= UnitRoster.SlotCount)
                return;

            if (!_roster.TryGet(kind, out _))
                return;

            if (!UnitSlotUnlockQuery.IsReady(kind, nodes, buildings))
                return;

            _progress.SetUnitSlot(slot, kind);
        }

        private void Clear(int slot)
        {
            if (slot < 0 || slot >= UnitRoster.SlotCount)
                return;

            _progress.ClearUnitSlot(slot);
        }
    }
}
