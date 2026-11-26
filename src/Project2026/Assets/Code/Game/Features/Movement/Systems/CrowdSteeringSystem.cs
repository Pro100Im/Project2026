using Code.Game.Common.Time;
using Code.Game.Features.Level;
using Entitas;
using UnityEngine;

namespace Code.Game.Features.Movement.Systems
{
    /// <summary>
    /// Moves every unit one simulation step: desired direction (straight to a point or along a navigation
    /// field), flowing around neighbours with a side that is chosen once and kept, inertia, then a step that is
    /// clamped by walls.
    /// Decisions are deliberately sticky: a unit that makes no progress stops for a moment and then commits to
    /// the other side instead of twitching between options.
    /// </summary>
    public class CrowdSteeringSystem : IExecuteSystem
    {
        // Distance to a neighbour at which a unit starts to steer around it.
        private const float AvoidMargin = 0.14f;

        // A neighbour this close and straight ahead stops the unit instead of being shoved.
        private const float BlockGap = 0.03f;
        private const float BlockAhead = 0.6f;

        private const float ArriveRadius = 0.03f;
        private const float SlowRadius = 0.15f;

        // Speeds are reached in about 1 / Acceleration seconds.
        private const float Acceleration = 8f;

        private const float SideCommitTime = 1f;
        private const float StuckTime = 1f;
        private const float StuckSpeedRatio = 0.2f;
        private const float PauseBase = 0.3f;
        private const float PauseStep = 0.1f;

        // Detour around a wall on the way to a point: 65 degrees off the straight line.
        private const float DetourTime = 1.2f;
        private const float DetourCos = 0.4226f;
        private const float DetourSin = 0.9063f;

        private const float MovingSpeedRatio = 0.25f;
        private const float IdleDelay = 0.2f;

        private readonly ITimeService _timeService;
        private readonly IGroup<GameEntity> _maps;

        public CrowdSteeringSystem(ITimeService timeService)
        {
            _timeService = timeService;
            _maps = Contexts.sharedInstance.game.GetGroup(GameMatcher.Arena);
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();

            if (map == null)
                return;

            var arena = map.arena.Value;
            var deltaTime = _timeService.DeltaTime;

            if (deltaTime <= 0f)
                return;

            for (var slot = 0; slot < arena.SlotLimit; slot++)
            {
                if (arena.UnitAlive[slot] == 0)
                    continue;

                Step(arena, slot, deltaTime);
            }
        }

        private static void Step(LevelArena arena, int slot, float deltaTime)
        {
            var position = arena.UnitPos[slot];
            var wallRadius = arena.UnitWallRadius[slot];

            arena.UnitPrevPos[slot] = position;

            // A building appeared on top of the unit: move it to the nearest valid place once.
            if (!CrowdCollision.IsFree(arena, position, wallRadius)
                && CrowdCollision.TrySettle(arena, position, wallRadius, out var settled))
            {
                position = settled;
                arena.UnitPos[slot] = position;
            }

            if (arena.UnitSideTimer[slot] > 0f)
                arena.UnitSideTimer[slot] -= deltaTime;

            var speed = arena.UnitSpeed[slot];
            var mode = arena.UnitMode[slot];

            if (mode == UnitModes.Hold || speed <= 0f)
            {
                arena.UnitVel[slot] = Vector2.zero;
                arena.UnitStuckTimer[slot] = 0f;
                arena.UnitPauseTimer[slot] = 0f;
                MarkMoving(arena, slot, 0f, speed, deltaTime);
                return;
            }

            var want = Vector2.zero;
            var wantSpeed = 0f;

            if (mode == UnitModes.Point)
            {
                var toGoal = arena.UnitGoal[slot] - position;
                var distance = toGoal.magnitude;

                if (distance > ArriveRadius)
                {
                    want = toGoal / distance;
                    wantSpeed = Mathf.Min(speed * (0.25f + (distance / SlowRadius)), speed);

                    // Never overshoot the point in one step.
                    wantSpeed = Mathf.Min(wantSpeed, distance / deltaTime);

                    // The straight line was blocked: walk at an angle to it for a while to get around the corner.
                    if (arena.UnitDetourTimer[slot] > 0f)
                    {
                        arena.UnitDetourTimer[slot] -= deltaTime;
                        want = Rotate(want, arena.UnitSide[slot] > 0 ? DetourSin : -DetourSin);
                    }
                }
            }
            else
            {
                want = SampleFlow(arena, slot, position);

                if (want.sqrMagnitude > 0f)
                    wantSpeed = speed;
            }

            var wantsToMove = wantSpeed > 0f;

            // "Thinking": the unit made no progress for a while, so it stands still for a moment.
            if (arena.UnitPauseTimer[slot] > 0f)
            {
                arena.UnitPauseTimer[slot] -= deltaTime;
                wantSpeed = 0f;
            }

            var velocity = arena.UnitVel[slot];
            var desired = Vector2.zero;

            if (wantSpeed > 0f)
                desired = Avoid(arena, slot, position, want) * wantSpeed;

            velocity = Vector2.MoveTowards(velocity, desired, speed * Acceleration * deltaTime);

            if (desired.sqrMagnitude == 0f && velocity.sqrMagnitude < 1e-6f)
                velocity = Vector2.zero;

            var moved = CrowdCollision.Move(arena, position, velocity * deltaTime, wallRadius);
            var actual = (moved - position) / deltaTime;

            // Whatever a wall took away is gone, so the unit does not keep pressing into it.
            arena.UnitVel[slot] = actual;
            arena.UnitPos[slot] = moved;

            var actualSpeed = actual.magnitude;

            UpdateStuck(arena, slot, wantsToMove, actualSpeed, speed, deltaTime);
            MarkMoving(arena, slot, actualSpeed, speed, deltaTime);
        }

        /// <summary>
        /// Direction along the unit's navigation field, blended over the four nearest cells so the path is a
        /// curve and not a chain of 45 degree segments. Zero when the unit stands on its goal.
        /// </summary>
        private static Vector2 SampleFlow(LevelArena arena, int slot, Vector2 position)
        {
            // A field for a body k cells wide is indexed by the origin cell of its k x k footprint, so the body
            // centre is shifted to the centre of that cell before the lookup. If the body stands where its own
            // field has no path, it falls back to the field of a narrower body.
            var cellSize = arena.CellSize;
            var field = arena.UnitField[slot];
            var offset = -1;
            var shift = 0f;
            var cell = -1;

            for (var clearance = arena.UnitClearance[slot]; clearance >= 1; clearance--)
            {
                var candidateShift = (clearance - 1) * cellSize * 0.5f;
                var candidateCell = arena.IndexAt(position.x - candidateShift, position.y - candidateShift);

                if (candidateCell < 0)
                    continue;

                var candidate = LevelArena.FieldSlot(field, clearance) * arena.CellCount;

                if (arena.Integration[candidate + candidateCell] == LevelArena.Unreachable)
                    continue;

                offset = candidate;
                shift = candidateShift;
                cell = candidateCell;
                break;
            }

            if (offset < 0 || arena.Integration[offset + cell] == 0)
                return Vector2.zero;

            var anchor = new Vector2(position.x - shift, position.y - shift);
            var gridX = ((anchor.x - arena.WorldMin.x) / cellSize) - 0.5f;
            var gridY = ((anchor.y - arena.WorldMin.y) / cellSize) - 0.5f;
            var x0 = Mathf.FloorToInt(gridX);
            var y0 = Mathf.FloorToInt(gridY);
            var tx = gridX - x0;
            var ty = gridY - y0;
            var sum = Vector2.zero;

            for (var corner = 0; corner < 4; corner++)
            {
                var x = x0 + (corner & 1);
                var y = y0 + (corner >> 1);

                if (x < 0 || y < 0 || x >= arena.Width || y >= arena.Height)
                    continue;

                var index = x + (y * arena.Width);

                if (arena.Integration[offset + index] == LevelArena.Unreachable)
                    continue;

                var weight = ((corner & 1) == 0 ? 1f - tx : tx) * ((corner >> 1) == 0 ? 1f - ty : ty);
                var direction = arena.FlowDir[offset + index];

                if (direction != LevelArena.NoDirection)
                {
                    sum += LevelArena.DirVector[direction] * weight;
                    continue;
                }

                // A goal cell pulls towards its centre.
                var toCenter = arena.CellCenter(index) - anchor;
                var length = toCenter.magnitude;

                if (length > 1e-4f)
                    sum += toCenter * (weight / length);
            }

            if (sum.sqrMagnitude > 1e-4f)
                return sum.normalized;

            var own = arena.FlowDir[offset + cell];

            return own != LevelArena.NoDirection ? LevelArena.DirVector[own] : Vector2.zero;
        }

        /// <summary>
        /// Bends the wanted direction around neighbours. Returns a unit vector, or zero when the way is blocked
        /// even after stepping aside, in which case the unit waits instead of shoving.
        /// </summary>
        private static Vector2 Avoid(LevelArena arena, int slot, Vector2 position, Vector2 want)
        {
            var radius = arena.UnitRadius[slot];
            var reach = radius + arena.MaxUnitRadius + AvoidMargin;
            var count = arena.Query(position.x - reach, position.y - reach, position.x + reach, position.y + reach);
            var steer = Vector2.zero;

            for (var i = 0; i < count; i++)
            {
                var other = arena.QuerySlots[i];

                if (other < 0 || other == slot || arena.UnitAlive[other] == 0)
                    continue;

                var offset = arena.UnitPos[other] - position;
                var distance = offset.magnitude;
                var gap = distance - radius - arena.UnitRadius[other];

                if (gap > AvoidMargin)
                    continue;

                var normal = distance > 1e-4f ? offset / distance : PseudoDirection(slot, other);
                var weight = 1f - Mathf.Clamp01(gap / AvoidMargin);
                var ahead = Vector2.Dot(normal, want);
                var standing = arena.UnitMoving[other] == 0;

                if (ahead > 0.1f)
                {
                    var side = ResolveSide(arena, slot, want, normal);
                    var tangent = side > 0 ? new Vector2(-normal.y, normal.x) : new Vector2(normal.y, -normal.x);

                    steer += tangent * (weight * ahead * (standing ? 1.5f : 0.7f));
                }

                steer -= normal * (weight * (standing ? 0.3f : 0.5f));
            }

            var direction = want + steer;
            var sqrLength = direction.sqrMagnitude;

            if (sqrLength < 1e-4f)
                return Vector2.zero;

            direction /= Mathf.Sqrt(sqrLength);

            // Second look with the final direction: is somebody standing right where the unit is about to step?
            for (var i = 0; i < count; i++)
            {
                var other = arena.QuerySlots[i];

                if (other < 0 || other == slot || arena.UnitAlive[other] == 0 || arena.UnitMoving[other] != 0)
                    continue;

                var offset = arena.UnitPos[other] - position;
                var distance = offset.magnitude;

                if (distance < 1e-4f || distance - radius - arena.UnitRadius[other] > BlockGap)
                    continue;

                if (Vector2.Dot(offset / distance, direction) > BlockAhead)
                    return Vector2.zero;
            }

            return direction;
        }

        /// <summary>
        /// Side on which the unit passes obstacles: +1 left, -1 right. Chosen at the first contact and then kept
        /// for a while, which is what stops left-right dithering in front of an obstacle.
        /// </summary>
        private static int ResolveSide(LevelArena arena, int slot, Vector2 want, Vector2 normal)
        {
            var side = (int)arena.UnitSide[slot];

            if (side != 0 && arena.UnitSideTimer[slot] > 0f)
                return side;

            var cross = (want.x * normal.y) - (want.y * normal.x);

            // The obstacle is to the left: pass on the right, and the other way round.
            // Dead ahead: everybody keeps to the right, which also resolves head-on meetings.
            side = cross < -0.05f ? 1 : -1;

            arena.UnitSide[slot] = (sbyte)side;
            arena.UnitSideTimer[slot] = SideCommitTime;

            return side;
        }

        private static void UpdateStuck(
            LevelArena arena,
            int slot,
            bool wantsToMove,
            float actualSpeed,
            float speed,
            float deltaTime)
        {
            if (!wantsToMove || arena.UnitPauseTimer[slot] > 0f)
                return;

            if (actualSpeed > speed * StuckSpeedRatio)
            {
                arena.UnitStuckTimer[slot] = Mathf.Max(arena.UnitStuckTimer[slot] - (deltaTime * 2f), 0f);
                return;
            }

            arena.UnitStuckTimer[slot] += deltaTime;

            if (arena.UnitStuckTimer[slot] < StuckTime)
                return;

            // No progress: stand for a moment, then try the other side and stay with it.
            arena.UnitStuckTimer[slot] = 0f;
            arena.UnitPauseTimer[slot] = PauseBase + ((arena.UnitEntityId[slot] & 3) * PauseStep);
            arena.UnitSide[slot] = (sbyte)(arena.UnitSide[slot] > 0 ? -1 : 1);
            arena.UnitSideTimer[slot] = SideCommitTime * 2f;
            arena.UnitRethinks[slot]++;

            if (arena.UnitMode[slot] == UnitModes.Point)
                arena.UnitDetourTimer[slot] = DetourTime;
        }

        private static Vector2 Rotate(Vector2 vector, float sin)
        {
            return new Vector2(
                (vector.x * DetourCos) - (vector.y * sin),
                (vector.x * sin) + (vector.y * DetourCos));
        }

        /// <summary>Moving flag with a delay on the way down, so walk and idle animations do not flicker.</summary>
        private static void MarkMoving(LevelArena arena, int slot, float actualSpeed, float speed, float deltaTime)
        {
            if (speed > 0f && actualSpeed > speed * MovingSpeedRatio)
            {
                arena.UnitMoving[slot] = 1;
                arena.UnitIdleTimer[slot] = 0f;
                return;
            }

            arena.UnitIdleTimer[slot] += deltaTime;

            if (arena.UnitIdleTimer[slot] >= IdleDelay)
                arena.UnitMoving[slot] = 0;
        }

        /// <summary>
        /// Stable direction from unit a to unit b for the case when they stand on the same point.
        /// Antisymmetric, so the two units agree on which way each of them goes.
        /// </summary>
        public static Vector2 PseudoDirection(int a, int b)
        {
            var low = Mathf.Min(a, b);
            var high = Mathf.Max(a, b);
            var angle = ((low * 73856093) ^ (high * 19349663)) & 1023;
            var radians = angle * (Mathf.PI * 2f / 1024f);
            var direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));

            return a < b ? direction : -direction;
        }
    }
}
