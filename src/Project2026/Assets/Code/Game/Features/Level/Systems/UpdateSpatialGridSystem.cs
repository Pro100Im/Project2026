using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Level.Systems
{
    /// <summary>
    /// Rebuilds the arena spatial grid from every targetable entity. A unit covers its body circle,
    /// a building covers its whole footprint.
    /// </summary>
    public class UpdateSpatialGridSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _targetables;
        private readonly IGroup<GameEntity> _maps;

        private readonly List<GameEntity> _buffer = new(LevelBuffers.DefaultCapacity);

        public UpdateSpatialGridSystem()
        {
            var gameContext = Contexts.sharedInstance.game;

            _targetables = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.CurrentCell,
                    GameMatcher.Targetable,
                    GameMatcher.Id)
                .NoneOf(GameMatcher.Dead));

            _maps = gameContext.GetGroup(GameMatcher.Arena);
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();

            if (map == null)
                return;

            var arena = map.arena.Value;
            var targetables = _targetables.GetEntities(LevelBuffers.Fit(_buffer, arena));

            arena.GridBegin();

            for (var i = 0; i < targetables.Count; i++)
            {
                var target = targetables[i];

                if (target.hasSimSlot)
                {
                    var slot = target.simSlot.Value;
                    var position = arena.UnitPos[slot];
                    var radius = arena.UnitRadius[slot];

                    arena.GridAdd(
                        target.id.Value,
                        slot,
                        position.x - radius,
                        position.y - radius,
                        position.x + radius,
                        position.y + radius);

                    continue;
                }

                var size = target.hasUnitSize ? target.unitSize.Value : Vector2Int.one;
                var cell = target.currentCell.Value;
                var min = arena.CellMin(cell.x, cell.y);
                var max = arena.CellMin(cell.x + size.x, cell.y + size.y);

                // Shrink a hair so a footprint that ends exactly on a bucket border does not spill over.
                const float inset = 0.001f;

                arena.GridAdd(target.id.Value, -1, min.x + inset, min.y + inset, max.x - inset, max.y - inset);
            }

            arena.GridBuild();
        }
    }
}
