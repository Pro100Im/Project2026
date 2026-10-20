using Code.Game.Features.Unit;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Research.Systems
{
    public class ResolveUnitSlotSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _bonuses;
        private readonly IGroup<GameEntity> _slots;
        private readonly List<GameEntity> _buffer = new(4);

        public ResolveUnitSlotSystem()
        {
            var game = Contexts.sharedInstance.game;
            _bonuses = game.GetGroup(GameMatcher.ResearchBonus);
            _slots = game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.UnitSlot,
                    GameMatcher.DefaultUnitConfig));
        }

        public void Execute()
        {
            var bonus = _bonuses.GetSingleEntity();
            var unlocked = bonus != null && bonus.hasUnlockedKnight ? bonus.unlockedKnight.Value : null;
            var slots = _slots.GetEntities(_buffer);

            for (var i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                var resolved = slot.defaultUnitConfig.Value;

                if (unlocked != null && slot.hasUnitKind && slot.unitKind.Value == UnitKind.Knight)
                    resolved = unlocked;

                if (slot.hasResolvedUnitConfig && slot.resolvedUnitConfig.Value == resolved)
                    continue;

                if (slot.hasResolvedUnitConfig)
                    slot.ReplaceResolvedUnitConfig(resolved);
                else
                    slot.AddResolvedUnitConfig(resolved);
            }
        }
    }
}
