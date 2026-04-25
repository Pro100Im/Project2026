using Entitas;
using UnityEngine;

namespace Code.Game.Features.Movement.Systems
{
    public class UpdateCellSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _units;
        private readonly IGroup<GameEntity> _maps;

        public UpdateCellSystem(GameContext context)
        {
            _units = context.GetGroup(GameMatcher.Transform);
            _maps = context.GetGroup(GameMatcher.TilemapMovement);
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();
            var tilemap = map.tilemapMovement.Value;

            foreach (var unit in _units)
            {
                var pos = unit.transform.Value.position;

                var bestCell = Vector3Int.zero;
                var bestDist = float.MaxValue;

                foreach (var kvp in tilemap)
                {
                    var d = (kvp.Value - pos).sqrMagnitude;

                    if (d < bestDist)
                    {
                        bestDist = d;
                        bestCell = kvp.Key;
                    }
                }

                if (!unit.hasCurrentCell || unit.currentCell.Value != bestCell)
                {
                    unit.ReplaceCurrentCell(bestCell);
                }
            }
        }
    }
}