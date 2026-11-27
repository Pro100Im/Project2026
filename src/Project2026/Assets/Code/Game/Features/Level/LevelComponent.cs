using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Level
{
    [Game] public class SpawnMap : IComponent { public Dictionary<Vector3Int, Vector3> Value; }
    [Game] public class Arena : IComponent { public LevelArena Value; }
}
