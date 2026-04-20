using Entitas;
using UnityEngine;

namespace Code.Game.Features.Spawn
{
    [Game] public class SpawnRequsted : IComponent { }
    [Game] public class WaitingToSpawn : IComponent { }
    [Game] public class SpawnPosition : IComponent { public Vector3 Value; }
}