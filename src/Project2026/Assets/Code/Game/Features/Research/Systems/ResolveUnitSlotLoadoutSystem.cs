using Code.Game.StaticData.Configs;
using Code.Infrastructure.Persistence;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Research.Systems
{
    public class ResolveUnitSlotLoadoutSystem : IExecuteSystem
    {
        private readonly IUserProgressService _progress;
        private readonly UnitRoster _roster;
        private readonly IGroup<GameEntity> _nodes;
        private readonly IGroup<GameEntity> _slots;
        private readonly List<GameEntity> _nodesBuffer = new(32);
        private readonly List<GameEntity> _slotsBuffer = new(4);

        public ResolveUnitSlotLoadoutSystem(IUserProgressService progress, UnitRoster roster)
        {
            _progress = progress;
            _roster = roster;

            var game = Contexts.sharedInstance.game;
            _nodes = game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.ResearchNode,
                    GameMatcher.ResearchLevel,
                    GameMatcher.ResearchUnitSlotUnlock));
            _slots = game.GetGroup(GameMatcher.UnitSlot);
        }

        public void Execute()
        {
            if (_roster == null)
                return;

            var nodes = _nodes.GetEntities(_nodesBuffer);
            var slots = _slots.GetEntities(_slotsBuffer);

            for (var i = 0; i < slots.Count; i++)
                Apply(slots[i], nodes);
        }

        private void Apply(GameEntity slot, List<GameEntity> nodes)
        {
            if (!_progress.TryGetUnitSlot(slot.unitSlot.Value, out var kind)
                || !_roster.TryGet(kind, out var unit)
                || !UnitSlotUnlockQuery.IsReady(kind, nodes, _progress))
            {
                Clear(slot);
                return;
            }

            SetKind(slot, kind);
            SetConfig(slot, unit.Config);
            SetIcon(slot, unit.Icon);
            slot.isUnitSlotAvailable = true;
        }

        private static void Clear(GameEntity slot)
        {
            if (slot.hasUnitKind)
                slot.RemoveUnitKind();

            if (slot.hasDefaultUnitConfig)
                slot.RemoveDefaultUnitConfig();

            if (slot.hasResolvedUnitConfig)
                slot.RemoveResolvedUnitConfig();

            if (slot.hasUnitIcon)
                slot.RemoveUnitIcon();

            slot.isUnitSlotAvailable = false;
        }

        private static void SetKind(GameEntity slot, UnitKind kind)
        {
            if (slot.hasUnitKind && slot.unitKind.Value == kind)
                return;

            if (slot.hasUnitKind)
                slot.ReplaceUnitKind(kind);
            else
                slot.AddUnitKind(kind);
        }

        private static void SetConfig(GameEntity slot, EntityConfig config)
        {
            if (slot.hasDefaultUnitConfig && slot.defaultUnitConfig.Value == config)
                return;

            if (slot.hasDefaultUnitConfig)
                slot.ReplaceDefaultUnitConfig(config);
            else
                slot.AddDefaultUnitConfig(config);
        }

        private static void SetIcon(GameEntity slot, Sprite icon)
        {
            if (icon == null)
            {
                if (slot.hasUnitIcon)
                    slot.RemoveUnitIcon();

                return;
            }

            if (slot.hasUnitIcon && slot.unitIcon.Value == icon)
                return;

            if (slot.hasUnitIcon)
                slot.ReplaceUnitIcon(icon);
            else
                slot.AddUnitIcon(icon);
        }
    }
}
