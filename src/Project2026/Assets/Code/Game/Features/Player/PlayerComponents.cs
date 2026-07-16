using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Player
{
    [Game] public class PlayerComponent : IComponent { }
    [Game] public class PlayerCastle : IComponent { }
    [Game] public class CastleUnderAttack : IComponent { }
    [Game] public class PlayerCastleCells : IComponent { public List<Vector3Int> Value; }
}