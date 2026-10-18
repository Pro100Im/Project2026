using Entitas;
using UnityEngine;

namespace Code.Game.Features.HitReaction
{
    [Game] public class HitReactionRequest : IComponent { public Vector2 Value; }
    [Game] public class HitReaction : IComponent { public HitReactionView Value; }
}
