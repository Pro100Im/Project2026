using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Target
{
    [Game] public class Targetable : IComponent { }
    [Game] public class TargetCellRequest : IComponent { }
    [Game] public class TargetSelected : IComponent { }

    [Game] [Input] [Meta] public class TargetId : IComponent { public int Value; }
    [Game] public class TargetPoint : IComponent { public Vector2 Value; }
    [Game] public class TargetFlow : IComponent { public List<Vector3Int> Value; }
    [Game] public class DefenseFlow : IComponent { public List<Vector3Int> Value; }
    [Game] public class TargetCell : IComponent { public Vector3Int Value; }
    [Game] public class SurroundSlot : IComponent { public Vector3Int Value; }
    [Game] public class SurroundTargetId : IComponent { public int Value; }
    [Game] public class RallyToCastle : IComponent { }
    [Game] public class DefensePatrolIdleDuration : IComponent { public float Value; }
    [Game] public class DefensePatrolWait : IComponent { public float Value; }
    [Game] public class DefensePatrolCell : IComponent { public Vector3Int Value; }
}