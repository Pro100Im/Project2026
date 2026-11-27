using Code.Game.Common.Time;
using Entitas;
using Unity.Collections;
using UnityEngine;

namespace Code.Game.Features.Level.Systems
{
    /// <summary>
    /// Builds the navigation fields of the arena with Dijkstra over 8 neighbours (10 straight / 14 diagonal,
    /// no corner cutting). One field per goal set and clearance class. Allocation free: the heap lives in the arena.
    /// </summary>
    public class BuildNavFieldsSystem : IExecuteSystem
    {
        private const int StraightCost = 10;
        private const int DiagonalCost = 14;

        // A cell where one unit stands costs about as much as 2.5 extra cells of walking; several units
        // standing there make a detour of a dozen cells worth it.
        private const float CongestionCost = 25f;
        private const float MaxCongestion = 4f;

        // One field is rebuilt per interval; with 6 fields each is refreshed about every 0.6 s.
        private const float RefreshInterval = 0.1f;

        private readonly ITimeService _timeService;
        private readonly IGroup<GameEntity> _maps;

        public BuildNavFieldsSystem(ITimeService timeService)
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

            if (arena.FieldsDirty)
            {
                arena.FieldsDirty = false;

                for (var slot = 0; slot < NavField.Count * LevelArena.ClearanceClasses; slot++)
                    BuildSlot(arena, slot);

                return;
            }

            // Jams change slowly, so the fields follow them one at a time instead of all at once.
            arena.FieldTimer += _timeService.DeltaTime;

            if (arena.FieldTimer < RefreshInterval)
                return;

            arena.FieldTimer = 0f;

            BuildSlot(arena, arena.FieldCursor);

            arena.FieldCursor = (arena.FieldCursor + 1) % (NavField.Count * LevelArena.ClearanceClasses);
        }

        private static void BuildSlot(LevelArena arena, int slot)
        {
            var field = slot / LevelArena.ClearanceClasses;
            var clearance = (slot % LevelArena.ClearanceClasses) + 1;

            if (field == NavField.EnemyGoal)
                Build(arena, field, clearance, arena.EnemyGoals, arena.EnemyGoalCount);
            else
                Build(arena, field, clearance, arena.DefenseGoals, arena.DefenseGoalCount);
        }

        private static void Build(LevelArena arena, int field, int clearance, NativeArray<int> goals, int goalCount)
        {
            var offset = LevelArena.FieldSlot(field, clearance) * arena.CellCount;
            var integration = arena.Integration;
            var width = arena.Width;
            var height = arena.Height;

            for (var i = 0; i < arena.CellCount; i++)
            {
                integration[offset + i] = LevelArena.Unreachable;
                arena.FlowDir[offset + i] = LevelArena.NoDirection;
            }

            arena.HeapCount = 0;

            for (var i = 0; i < goalCount; i++)
                Seed(arena, offset, clearance, goals[i]);

            while (arena.HeapCount > 0)
            {
                var top = Pop(arena);
                var cost = (int)(top >> 32);
                var cell = (int)(top & 0xFFFFFFFF);

                if (cost > integration[offset + cell])
                    continue;

                var x = cell % width;
                var y = cell / width;

                for (var dir = 0; dir < 8; dir++)
                {
                    var neighbor = Step(arena, x, y, dir, clearance, width, height);

                    if (neighbor < 0)
                        continue;

                    var newCost = cost
                        + ((dir & 1) == 0 ? StraightCost : DiagonalCost)
                        + (int)(Mathf.Min(arena.Congestion[neighbor], MaxCongestion) * CongestionCost);

                    if (newCost >= integration[offset + neighbor])
                        continue;

                    integration[offset + neighbor] = newCost;
                    Push(arena, newCost, neighbor);
                }
            }

            for (var cell = 0; cell < arena.CellCount; cell++)
            {
                var cost = integration[offset + cell];

                if (cost == LevelArena.Unreachable || cost == 0)
                    continue;

                var x = cell % width;
                var y = cell / width;
                var bestCost = cost;
                var bestDir = LevelArena.NoDirection;

                for (var dir = 0; dir < 8; dir++)
                {
                    var neighbor = Step(arena, x, y, dir, clearance, width, height);

                    if (neighbor < 0)
                        continue;

                    var neighborCost = integration[offset + neighbor];

                    if (neighborCost >= bestCost)
                        continue;

                    bestCost = neighborCost;
                    bestDir = (byte)dir;
                }

                arena.FlowDir[offset + cell] = bestDir;
            }
        }

        /// <summary>
        /// Every footprint position that covers a goal cell is a seed. A goal cell inside a building (the castle)
        /// seeds the footprint positions right next to it instead.
        /// </summary>
        private static void Seed(LevelArena arena, int offset, int clearance, int goal)
        {
            var goalX = goal % arena.Width;
            var goalY = goal / arena.Width;
            var seeded = false;

            // Footprints (by origin cell) that cover the goal cell.
            for (var y = goalY - (clearance - 1); y <= goalY; y++)
            {
                for (var x = goalX - (clearance - 1); x <= goalX; x++)
                {
                    if (x < 0 || y < 0)
                        continue;

                    var cell = x + (y * arena.Width);

                    if (arena.Fit[cell] < clearance)
                        continue;

                    SeedCell(arena, offset, cell);
                    seeded = true;
                }
            }

            if (seeded)
                return;

            // The goal is inside a building: footprints that touch it from outside.
            for (var y = goalY - clearance; y <= goalY + 1; y++)
            {
                for (var x = goalX - clearance; x <= goalX + 1; x++)
                {
                    if (x < 0 || y < 0 || x >= arena.Width || y >= arena.Height)
                        continue;

                    var cell = x + (y * arena.Width);

                    if (arena.Fit[cell] >= clearance)
                        SeedCell(arena, offset, cell);
                }
            }
        }

        private static void SeedCell(LevelArena arena, int offset, int cell)
        {
            if (arena.Integration[offset + cell] == 0)
                return;

            arena.Integration[offset + cell] = 0;
            Push(arena, 0, cell);
        }

        /// <summary>Neighbour index in a direction, or -1 when the step is not allowed for this clearance.</summary>
        private static int Step(LevelArena arena, int x, int y, int dir, int clearance, int width, int height)
        {
            var dx = LevelArena.DirX[dir];
            var dy = LevelArena.DirY[dir];
            var nx = x + dx;
            var ny = y + dy;

            if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                return -1;

            var neighbor = nx + (ny * width);

            if (arena.Fit[neighbor] < clearance)
                return -1;

            if ((dir & 1) != 0)
            {
                if (arena.Fit[nx + (y * width)] < clearance || arena.Fit[x + (ny * width)] < clearance)
                    return -1;
            }

            return neighbor;
        }

        private static void Push(LevelArena arena, int cost, int cell)
        {
            var heap = arena.Heap;

            if (arena.HeapCount >= heap.Length)
            {
                arena.NoteOverflow();
                return;
            }

            var item = ((long)cost << 32) | (uint)cell;
            var index = arena.HeapCount++;

            while (index > 0)
            {
                var parent = (index - 1) >> 1;
                var parentItem = heap[parent];

                if (parentItem <= item)
                    break;

                heap[index] = parentItem;
                index = parent;
            }

            heap[index] = item;
        }

        private static long Pop(LevelArena arena)
        {
            var heap = arena.Heap;
            var top = heap[0];
            var count = --arena.HeapCount;

            if (count == 0)
                return top;

            var item = heap[count];
            var index = 0;

            while (true)
            {
                var child = (index << 1) + 1;

                if (child >= count)
                    break;

                if (child + 1 < count && heap[child + 1] < heap[child])
                    child++;

                var childItem = heap[child];

                if (childItem >= item)
                    break;

                heap[index] = childItem;
                index = child;
            }

            heap[index] = item;

            return top;
        }
    }
}
