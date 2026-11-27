using Code.Game.StaticData.Configs;
using System.Collections.Generic;

namespace Code.Game.Features.Level
{
    /// <summary>
    /// Capacity of the entity buffers systems pass to <c>GetEntities</c>. A buffer is sized for the level budget
    /// up front, so filling it never grows it while the level runs.
    /// </summary>
    public static class LevelBuffers
    {
        public const int DefaultCapacity = LevelConfig.DefaultMaxUnits + LevelArena.StaticItemsReserve;

        /// <summary>
        /// Raises the capacity to what the level can hold. Only a level configured above the default budget
        /// reallocates, and only on its first tick.
        /// </summary>
        public static List<T> Fit<T>(List<T> buffer, LevelArena arena)
        {
            if (buffer.Capacity < arena.MaxItems)
                buffer.Capacity = arena.MaxItems;

            return buffer;
        }
    }
}
