using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Target.Systems
{
    public class ReleaseSurroundSlotSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _units;
        private readonly IGroup<GameEntity> _maps;

        private readonly List<GameEntity> _buffer = new(256);

        public ReleaseSurroundSlotSystem(GameContext context)
        {
            _units = context.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.SurroundSlot,
                    GameMatcher.SurroundTargetId,
                    GameMatcher.Id));

            _maps = context.GetGroup(GameMatcher.AllOf(GameMatcher.SurroundField));
        }

        public void Execute()
        {
            var mapEntity = _maps.GetSingleEntity();

            if (mapEntity == null)
                return;

            var surroundField = mapEntity.surroundField.Value;
            var units = _units.GetEntities(_buffer);

            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                var shouldRelease = unit.isDead;

                if (!shouldRelease)
                {
                    var target = GetGameEntityById.Get(unit.surroundTargetId.Value);
                    shouldRelease = target == null || target.isDead;
                }

                if (!shouldRelease)
                    continue;

                var slot = unit.surroundSlot.Value;
                surroundField.Remove(slot);

                unit.RemoveSurroundSlot();
                unit.RemoveSurroundTargetId();

                if (unit.hasTargetId)
                    unit.RemoveTargetId();
            }
        }
    }
}
