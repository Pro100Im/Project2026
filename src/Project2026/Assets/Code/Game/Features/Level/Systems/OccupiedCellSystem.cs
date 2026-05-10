using Entitas;
using UnityEngine;

namespace Code.Game.Features.Level.Systems
{
    public class OccupiedCellSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _entities;
        private readonly IGroup<GameEntity> _maps;

        public OccupiedCellSystem(GameContext context)
        {
            _entities = context.GetGroup(GameMatcher
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
            var occupField = map.occupField.Value;

            occupField.Clear();

            foreach (var entity in _entities)
            {
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