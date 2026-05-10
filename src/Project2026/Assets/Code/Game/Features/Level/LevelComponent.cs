using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Level
{
    [Game] public class TilemapMovement : IComponent { public Dictionary<Vector3Int, Vector3> Value; }
    [Game] public class SpawnMap : IComponent { public Dictionary<Vector3Int, Vector3> Value; }
    [Game] public class FlowFields : IComponent { public Dictionary<Vector2Int, Dictionary<Vector3Int, Vector3Int>> Value; }
    [Game] public class IntegrationFields : IComponent { public Dictionary<Vector2Int, Dictionary<Vector3Int, int>> Value; }
    [Game] public class OccupField : IComponent { public Dictionary<Vector3Int, int> Value; }
    [Game] public class ReservedField : IComponent { public Dictionary<Vector3Int, int> Value; }
    [Game] public class SpatialHash : IComponent { public Dictionary<Vector2Int, List<int>> Value; }
    [Game] public class SpawnReservedField : IComponent { public HashSet<Vector3Int> Value; }
    [Game] public class FlowFieldDirty : IComponent { }
}