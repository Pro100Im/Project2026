using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Level
{
    [Game] public class TilemapMovement : IComponent { public Dictionary<Vector3, bool> Value; }
    [Game] public class OccupancyMap : IComponent { public Dictionary<Vector3, int> Value; }
    [Game] public class SpawnMap : IComponent { public List<Vector3> Value; }
    [Game] public class GridSize : IComponent { public Vector3 Value; }
}