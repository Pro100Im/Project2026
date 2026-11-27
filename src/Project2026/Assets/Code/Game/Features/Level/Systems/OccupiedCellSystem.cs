using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Level.Systems
{
    /// <summary>
    /// Stamps the footprint of every unit and building into <see cref="LevelArena.Occupant"/>, in place.
    /// Read by slot picking and spawning to keep footprints apart.
    /// </summary>
    public class OccupiedCellSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _cells;
        private readonly IGroup<GameEntity> _maps;

        private readonly List<GameEntity> _cellsBuffer = new(LevelBuffers.DefaultCapacity);

        public OccupiedCellSystem()
        {
            var gameContext = Contexts.sharedInstance.game;

            _cells = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.CurrentCell,
                GameMatcher.UnitSize,
                GameMatcher.Id));

            _maps = gameContext.GetGroup(GameMatcher.Arena);
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();

            if (map == null)
                return;

            var arena = map.arena.Value;
            var occupant = arena.Occupant;
            var cells = _cells.GetEntities(LevelBuffers.Fit(_cellsBuffer, arena));

            for (var i = 0; i < arena.CellCount; i++)
                occupant[i] = 0;

            for (var i = 0; i < cells.Count; i++)
            {
                var entity = cells[i];

                if (entity.isDead)
                    continue;

                var origin = entity.currentCell.Value;
                var size = entity.unitSize.Value;
                var id = entity.id.Value;

                for (var x = 0; x < size.x; x++)
                {
                    for (var y = 0; y < size.y; y++)
                    {
                        var index = arena.IndexOf(origin.x + x, origin.y + y);

                        if (index >= 0)
                            occupant[index] = id;
                    }
                }
            }
        }
    }
}
