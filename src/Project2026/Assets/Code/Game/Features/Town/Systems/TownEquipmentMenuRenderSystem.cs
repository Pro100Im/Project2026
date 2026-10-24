using Code.Game.Features.Research;
using Code.Game.StaticData.Configs;
using Code.Infrastructure.Persistence;
using Code.Meta.Features.Town;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Town.Systems
{
    public class TownEquipmentMenuRenderSystem : IExecuteSystem
    {
        private readonly IUserProgressService _progress;
        private readonly UnitRoster _roster;
        private readonly IGroup<TownEntity> _menus;
        private readonly IGroup<TownEntity> _nodes;
        private readonly IGroup<TownEntity> _buildings;
        private readonly List<TownEntity> _nodesBuffer = new(32);
        private readonly List<TownEntity> _buildingsBuffer = new(16);
        private readonly List<TownEquipmentMenu.EquipmentChoice> _choices = new(4);

        public TownEquipmentMenuRenderSystem(IUserProgressService progress, UnitRoster roster)
        {
            _progress = progress;
            _roster = roster;

            var town = Contexts.sharedInstance.town;
            _menus = town.GetGroup(TownMatcher.TownEquipmentMenu);
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

        public void Execute()
        {
            var menuEntity = _menus.GetSingleEntity();

            if (menuEntity == null || !menuEntity.isTownEquipmentMenuOpened || !menuEntity.hasTownEquipmentMenu || _roster == null)
                return;

            var menu = menuEntity.townEquipmentMenu.Value;
            var nodes = _nodes.GetEntities(_nodesBuffer);
            var buildings = _buildings.GetEntities(_buildingsBuffer);

            for (var i = 0; i < UnitRoster.SlotCount; i++)
            {
                if (_progress.TryGetUnitSlot(i, out var kind)
                    && _roster.TryGet(kind, out var entry)
                    && UnitSlotUnlockQuery.IsReady(kind, nodes, buildings))
                {
                    menu.SetSlot(i, entry.Icon, entry.Title, true, kind);
                    continue;
                }

                menu.SetSlot(i, _roster.LockedIcon, "Empty", false, default);
            }

            _choices.Clear();

            for (var i = 0; i < _roster.Count; i++)
            {
                var entry = _roster.Get(i);

                if (entry == null || !UnitSlotUnlockQuery.IsReady(entry.Kind, nodes, buildings))
                    continue;

                _choices.Add(new TownEquipmentMenu.EquipmentChoice(entry.Kind, entry.Icon, entry.Title));
            }

            menu.SetChoices(_choices);
        }
    }
}
