using Code.Game.Features.Level;
using Entitas;
using UnityEngine;

namespace Code.Game.Features.Movement.Systems
{
    /// <summary>
    /// Keeps bodies apart after the steering step, so a crowd packs shoulder to shoulder but does not collapse
    /// into one point. Units that hold a position are not moved at all: others give way to them.
    /// The correction goes through the wall-clamped move, so a crowd pressing on a wall compresses a little
    /// rather than squeezing anybody out of the map or into a building.
    /// </summary>
    public class CrowdSeparationSystem : IExecuteSystem
    {
        // Allowed overlap: bodies may sink into each other up to this share of the sum of radii.
        private const float SoftContact = 0.8f;
        private const float HardContact = 0.95f;
        private const float MaxPush = 0.1f;
        private const float QueryPadding = 0.1f;

        private readonly IGroup<GameEntity> _maps;

        public CrowdSeparationSystem()
        {
            _maps = Contexts.sharedInstance.game.GetGroup(GameMatcher.Arena);
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();

            if (map == null)
                return;

            var arena = map.arena.Value;

            for (var slot = 0; slot < arena.SlotLimit; slot++)
            {
                if (arena.UnitAlive[slot] == 0 || arena.UnitImmovable[slot] != 0)
                    continue;

                Separate(arena, slot);
            }
        }

        private static void Separate(LevelArena arena, int slot)
        {
            var position = arena.UnitPos[slot];
            var radius = arena.UnitRadius[slot];
            var reach = radius + arena.MaxUnitRadius + QueryPadding;
            var count = arena.Query(position.x - reach, position.y - reach, position.x + reach, position.y + reach);
            var push = Vector2.zero;

            for (var i = 0; i < count; i++)
            {
                var other = arena.QuerySlots[i];

                if (other < 0 || other == slot || arena.UnitAlive[other] == 0)
                    continue;

                var immovable = arena.UnitImmovable[other] != 0;
                var minDistance = (radius + arena.UnitRadius[other]) * (immovable ? HardContact : SoftContact);
                var offset = position - arena.UnitPos[other];
                var sqrDistance = offset.sqrMagnitude;

                if (sqrDistance >= minDistance * minDistance)
                    continue;

                var distance = Mathf.Sqrt(sqrDistance);
                var normal = distance > 1e-4f ? offset / distance : -CrowdSteeringSystem.PseudoDirection(slot, other);

                // Two free units share the correction, a holding unit gives none of it.
                push += normal * ((minDistance - distance) * (immovable ? 1f : 0.5f));
            }

            var sqrPush = push.sqrMagnitude;

            if (sqrPush < 1e-8f)
                return;

            var length = Mathf.Sqrt(sqrPush);

            if (length > MaxPush)
            {
                push *= MaxPush / length;
                length = MaxPush;
            }

            arena.UnitPos[slot] = CrowdCollision.Move(arena, position, push, arena.UnitWallRadius[slot]);

            // Drop the part of the velocity that runs into the contact, otherwise the unit would walk back
            // into its neighbour on the next step and vibrate.
            var pushNormal = push / length;
            var velocity = arena.UnitVel[slot];
            var into = Vector2.Dot(velocity, pushNormal);

            if (into < 0f)
                arena.UnitVel[slot] = velocity - (pushNormal * into);
        }
    }
}
