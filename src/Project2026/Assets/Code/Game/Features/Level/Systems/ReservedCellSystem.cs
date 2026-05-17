using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Level.Systems
{
    public class ReservedCellSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _units;
        private readonly IGroup<GameEntity> _maps;

        private readonly List<GameEntity> _unitsBuffer = new(512);

        public ReservedCellSystem(GameContext context)
        {
            _units = context.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.TargetCell,
                GameMatcher.UnitSize,
                GameMatcher.MovementAvailable,
                GameMatcher.GridMovement,
                GameMatcher.Id));

            _maps = context.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.ReservedField));
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();

            if (map == null)
                return;

            var reservedField = map.reservedField.Value;
            var units = _units.GetEntities(_unitsBuffer);

            reservedField.Clear();

            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];

                if (unit.isDead)
                    continue;

                var targetOrigin = unit.targetCell.Value;
                var size = unit.unitSize.Value;

                for (var x = 0; x < size.x; x++)
                {
                    for (var y = 0; y < size.y; y++)
                    {
                        var cell = new Vector3Int(targetOrigin.x + x, targetOrigin.y + y, targetOrigin.z);

                        reservedField[cell] = unit.id.Value;
                    }
                }
            }
        }
    }
}