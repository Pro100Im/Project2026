using Code.Game.Features.Level;
using UnityEngine;

namespace Code.Game.Features.Spawn.Systems
{
    /// <summary>Footprint checks for spawn points against the arena: tilemap, standing bodies, this frame's spawns.</summary>
    public static class SpawnCells
    {
        public static bool CanFit(LevelArena arena, Vector3Int origin, Vector2Int size)
        {
            for (var x = 0; x < size.x; x++)
            {
                for (var y = 0; y < size.y; y++)
                {
                    var cell = new Vector3Int(origin.x + x, origin.y + y, origin.z);

                    if (!arena.IsTile(cell) || arena.OccupantAt(cell) != 0 || arena.IsSpawnReserved(cell))
                        return false;
                }
            }

            return true;
        }

        public static void Reserve(LevelArena arena, Vector3Int origin, Vector2Int size)
        {
            for (var x = 0; x < size.x; x++)
            {
                for (var y = 0; y < size.y; y++)
                    arena.ReserveSpawn(new Vector3Int(origin.x + x, origin.y + y, origin.z));
            }
        }
    }
}
