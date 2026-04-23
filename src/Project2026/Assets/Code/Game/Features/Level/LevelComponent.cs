using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Level
{
    [Game] public class TilemapMovement : IComponent { public Dictionary<Vector3Int, Vector3> Value; }
    [Game] public class SpawnMap : IComponent { public Dictionary<Vector3Int, Vector3> Value; }
    [Game] public class FlowField : IComponent { public Dictionary<Vector3Int, Vector3Int> Value; }
    [Game] public class FlowFieldDirty : IComponent { }
}