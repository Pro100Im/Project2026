using Entitas;
using UnityEngine;

namespace Code.Game.Features.Spawn
{
    public class SpawnComponents : MonoBehaviour
    {
        [Game] public class SpawnRequsted : IComponent { }
        [Game] public class WaitingToSpawn : IComponent { }
        [Game] public class ForPlayer : IComponent { }
        [Game] public class FreePoint : IComponent { }
        [Game] public class SpawnPositions : IComponent { public Vector3[] Value; }
        [Game] public class SpawnPosition : IComponent { public Vector3 Value; }
    }
}