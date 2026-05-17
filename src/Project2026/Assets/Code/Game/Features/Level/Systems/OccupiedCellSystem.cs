using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Level.Systems
{
    public class OccupiedCellSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _cells;
        private readonly IGroup<GameEntity> _maps;

        private readonly List<GameEntity> _cellsBuffer = new(512);

        public OccupiedCellSystem(GameContext context)
        {
            _cells = context.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.CurrentCell,
                GameMatcher.UnitSize,
                GameMatcher.Id));

            _maps = context.GetGroup(GameMatcher
                .AllOf(GameMatcher.OccupField));
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();

            if (map == null)
                return;

            var occupField = map.occupField.Value;
            var cells = _cells.GetEntities(_cellsBuffer);

            occupField.Clear();

            for (var i = 0; i < cells.Count; i++)
            {
                var entity = cells[i];

                if (entity.isMoving || entity.isDead)
                    continue;

                var origin = entity.currentCell.Value;
                var sizeX = entity.unitSize.Value.x;
                var sizeY = entity.unitSize.Value.y;

                for (var x = 0; x < sizeX; x++)
                {
                    for (var y = 0; y < sizeY; y++)
                    {
                        var cell = new Vector3Int(origin.x + x, origin.y + y, origin.z);

                        occupField[cell] = entity.id.Value;
                    }
                }
            }
        }
    }
}