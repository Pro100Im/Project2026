using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Level
{
    [Game] public class TilemapMovement : IComponent { public Dictionary<Vector3Int, Vector3> Value; }
    [Game] public class OccupancyMap : IComponent { public Dictionary<Vector3Int, int> Value; }
    [Game] public class SpawnMap : IComponent { public Dictionary<Vector3Int, Vector3> Value; }
    [Game] public class GridSize : IComponent { public Vector3 Value; }
}