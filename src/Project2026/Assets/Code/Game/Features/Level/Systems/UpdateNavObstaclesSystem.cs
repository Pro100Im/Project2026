using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Level.Systems
{
    /// <summary>
    /// Stamps buildings (castle, towers, tower places) into the navigation grid as walls and derives the
    /// clearance map. Runs only when the set of buildings changes.
    /// </summary>
    public class UpdateNavObstaclesSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _maps;
        private readonly IGroup<GameEntity> _statics;

        private readonly List<GameEntity> _buffer = new(64);

        public UpdateNavObstaclesSystem()
        {
            var gameContext = Contexts.sharedInstance.game;

            _maps = gameContext.GetGroup(GameMatcher.Arena);

            _statics = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.CurrentCell,
                    GameMatcher.UnitSize,
                    GameMatcher.Id)
                .NoneOf(
                    GameMatcher.GridMovement,
                    GameMatcher.Unit));
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();

            if (map == null)
                return;

            var arena = map.arena.Value;
            var statics = _statics.GetEntities(_buffer);
            var signature = GetSignature(statics);

            if (!arena.ObstaclesDirty && signature == arena.ObstacleSignature)
                return;

            arena.ObstacleSignature = signature;
            arena.ObstaclesDirty = false;
            arena.FieldsDirty = true;

            for (var i = 0; i < arena.CellCount; i++)
                arena.Blocked[i] = 0;

            for (var i = 0; i < statics.Count; i++)
            {
                var entity = statics[i];

                if (entity.isDead)
                    continue;

                var origin = entity.currentCell.Value;
                var size = entity.unitSize.Value;

                for (var x = 0; x < size.x; x++)
                {
                    for (var y = 0; y < size.y; y++)
                    {
                        var index = arena.IndexOf(origin.x + x, origin.y + y);

                        if (index >= 0)
                            arena.Blocked[index] = 1;
                    }
                }
            }

            BuildClearance(arena);
            BuildFit(arena);
        }

        private static int GetSignature(List<GameEntity> statics)
        {
            var signature = 17;

            for (var i = 0; i < statics.Count; i++)
            {
                var entity = statics[i];

                if (entity.isDead)
                    continue;

                var cell = entity.currentCell.Value;
                var size = entity.unitSize.Value;

                unchecked
                {
                    var item = entity.id.Value;

                    item = (item * 31) + cell.x;
                    item = (item * 31) + cell.y;
                    item = (item * 31) + size.x;
                    item = (item * 31) + size.y;

                    // Order independent: group order is not stable.
                    signature += item * 486187739;
                }
            }

            return signature;
        }

        /// <summary>
        /// Chebyshev distance to the nearest wall, in cells: 0 = wall, 1 = walkable cell touching a wall.
        /// Everything outside the grid counts as wall.
        /// </summary>
        private static void BuildClearance(LevelArena arena)
        {
            var width = arena.Width;
            var height = arena.Height;
            var clearance = arena.Clearance;

            for (var i = 0; i < arena.CellCount; i++)
                clearance[i] = (byte)(arena.Tile[i] != 0 && arena.Blocked[i] == 0 ? 254 : 0);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var index = x + (y * width);
                    int value = clearance[index];

                    if (value == 0)
                        continue;

                    var best = Read(clearance, x - 1, y, width, height);

                    best = Min(best, Read(clearance, x - 1, y - 1, width, height));
                    best = Min(best, Read(clearance, x, y - 1, width, height));
                    best = Min(best, Read(clearance, x + 1, y - 1, width, height));

                    if (best + 1 < value)
                        clearance[index] = (byte)(best + 1);
                }
            }

            for (var y = height - 1; y >= 0; y--)
            {
                for (var x = width - 1; x >= 0; x--)
                {
                    var index = x + (y * width);
                    int value = clearance[index];

                    if (value == 0)
                        continue;

                    var best = Read(clearance, x + 1, y, width, height);

                    best = Min(best, Read(clearance, x + 1, y + 1, width, height));
                    best = Min(best, Read(clearance, x, y + 1, width, height));
                    best = Min(best, Read(clearance, x - 1, y + 1, width, height));

                    if (best + 1 < value)
                        clearance[index] = (byte)(best + 1);
                }
            }
        }

        /// <summary>
        /// For every cell: the largest k for which the k x k block starting at the cell is walkable.
        /// </summary>
        private static void BuildFit(LevelArena arena)
        {
            var width = arena.Width;
            var height = arena.Height;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var fit = 0;

                    while (fit < LevelArena.ClearanceClasses && BlockRingWalkable(arena, x, y, fit + 1))
                        fit++;

                    arena.Fit[x + (y * width)] = (byte)fit;
                }
            }
        }

        /// <summary>The cells a block gains when it grows from size - 1 to size.</summary>
        private static bool BlockRingWalkable(LevelArena arena, int x, int y, int size)
        {
            var last = size - 1;

            if (x + last >= arena.Width || y + last >= arena.Height)
                return false;

            for (var i = 0; i <= last; i++)
            {
                if (arena.Clearance[x + last + ((y + i) * arena.Width)] == 0)
                    return false;

                if (arena.Clearance[x + i + ((y + last) * arena.Width)] == 0)
                    return false;
            }

            return true;
        }

        private static int Read(Unity.Collections.NativeArray<byte> clearance, int x, int y, int width, int height)
        {
            if (x < 0 || y < 0 || x >= width || y >= height)
                return 0;

            return clearance[x + (y * width)];
        }

        private static int Min(int a, int b) => a < b ? a : b;
    }
}
