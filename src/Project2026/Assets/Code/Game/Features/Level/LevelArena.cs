using System;
using Unity.Collections;
using UnityEngine;

namespace Code.Game.Features.Level
{
    /// <summary>
    /// Navigation fields of a level. Each field has one variant per clearance class.
    /// </summary>
    public static class NavField
    {
        public const int EnemyGoal = 0;
        public const int DefensePost = 1;

        public const int Count = 2;
    }

    public static class UnitModes
    {
        /// <summary>Stands still and cannot be pushed: attacking, frozen, holding a combat position.</summary>
        public const byte Hold = 0;

        /// <summary>Walks straight to <see cref="LevelArena.UnitGoal"/>.</summary>
        public const byte Point = 1;

        /// <summary>Follows the navigation field <see cref="LevelArena.UnitField"/>.</summary>
        public const byte Flow = 2;
    }

    public struct BucketRange
    {
        public short MinX;
        public short MinY;
        public short MaxX;
        public short MaxY;
    }

    /// <summary>
    /// All per-level simulation memory. Allocated once when the level map binds and freed when it unbinds;
    /// nothing in here grows or reallocates while the level runs.
    /// Data only: systems own the logic that fills and reads these arrays.
    /// </summary>
    public sealed class LevelArena : IDisposable
    {
        public const int Unreachable = int.MaxValue;
        public const int ClearanceClasses = 3;
        public const byte NoDirection = 255;

        private const int BucketCells = 2;
        private const int StaticItemsReserve = 64;
        private const int EntriesPerItem = 4;
        private const int StaticEntriesReserve = 512;

        // Offsets of the 8 neighbours. Even indices are straight steps, odd indices are diagonals.
        public static readonly int[] DirX = { 1, 1, 0, -1, -1, -1, 0, 1 };
        public static readonly int[] DirY = { 0, 1, 1, 1, 0, -1, -1, -1 };

        public static readonly Vector2[] DirVector =
        {
            new Vector2(1f, 0f),
            new Vector2(0.70710678f, 0.70710678f),
            new Vector2(0f, 1f),
            new Vector2(-0.70710678f, 0.70710678f),
            new Vector2(-1f, 0f),
            new Vector2(-0.70710678f, -0.70710678f),
            new Vector2(0f, -1f),
            new Vector2(0.70710678f, -0.70710678f)
        };

        public readonly int OriginX;
        public readonly int OriginY;
        public readonly int Width;
        public readonly int Height;
        public readonly int CellCount;
        public readonly float CellSize;
        public readonly Vector2 WorldMin;
        public readonly int MaxUnits;

        // Navigation grid, one element per cell.
        public NativeArray<byte> Tile;
        public NativeArray<byte> Blocked;
        public NativeArray<byte> Clearance;

        // Largest k (up to ClearanceClasses) for which the k x k block of cells starting at this cell is walkable.
        // A body k cells wide is navigated by the origin cell of its footprint.
        public NativeArray<byte> Fit;

        // Fields, laid out as [slot * CellCount + cell], slot = field * ClearanceClasses + (clearanceClass - 1).
        public NativeArray<int> Integration;
        public NativeArray<byte> FlowDir;

        public NativeArray<int> EnemyGoals;
        public NativeArray<int> DefenseGoals;
        public int EnemyGoalCount;
        public int DefenseGoalCount;

        public NativeArray<long> Heap;
        public int HeapCount;

        public bool ObstaclesDirty = true;
        public bool FieldsDirty = true;
        public int ObstacleSignature;

        // Spatial grid, rebuilt every tick with a counting sort.
        public readonly float BucketSize;
        public readonly int BucketsX;
        public readonly int BucketsY;
        public readonly int BucketCount;

        public NativeArray<int> BucketStart;
        public NativeArray<int> BucketFill;
        public NativeArray<int> Entries;
        public NativeArray<int> ItemId;
        public NativeArray<BucketRange> ItemRange;
        public NativeArray<int> ItemStamp;
        public NativeArray<int> ItemSlot;
        public NativeArray<int> QueryResults;
        public NativeArray<int> QuerySlots;
        public int ItemCount;
        public int EntryCount;

        // Units, structure of arrays indexed by sim slot. A slot is handed out when a unit starts moving on the
        // map and returned when it dies.
        public NativeArray<byte> UnitAlive;
        public NativeArray<int> UnitEntityId;
        public NativeArray<int> UnitSeen;
        public NativeArray<Vector2> UnitPos;
        public NativeArray<Vector2> UnitPrevPos;
        public NativeArray<Vector2> UnitVel;
        public NativeArray<Vector2> UnitGoal;
        public NativeArray<Vector2> UnitViewOffset;
        public NativeArray<float> UnitRadius;
        public NativeArray<float> UnitWallRadius;
        public NativeArray<float> UnitSpeed;
        public NativeArray<float> UnitSideTimer;
        public NativeArray<float> UnitStuckTimer;
        public NativeArray<float> UnitPauseTimer;
        public NativeArray<float> UnitDetourTimer;
        public NativeArray<float> UnitIdleTimer;
        public NativeArray<float> UnitFlipTimer;
        public NativeArray<byte> UnitMode;
        public NativeArray<byte> UnitField;
        public NativeArray<byte> UnitClearance;
        public NativeArray<byte> UnitMoving;
        public NativeArray<byte> UnitImmovable;
        public NativeArray<sbyte> UnitSide;
        public NativeArray<byte> UnitRethinks;
        public NativeArray<int> FreeSlots;
        public int FreeCount;
        public int SlotLimit;
        public int SyncStamp;
        public float MaxUnitRadius;

        // Smoothed density of standing units per cell; makes lasting jams expensive for the fields.
        public NativeArray<float> Congestion;
        public int FieldCursor;
        public float FieldTimer;

        private int _queryStamp;
        private bool _disposed;

        public LevelArena(int originX, int originY, int width, int height, float cellSize, Vector2 worldMin, int maxUnits)
        {
            OriginX = originX;
            OriginY = originY;
            Width = width;
            Height = height;
            CellCount = width * height;
            CellSize = cellSize;
            WorldMin = worldMin;
            MaxUnits = maxUnits;

            const Allocator allocator = Allocator.Persistent;

            Tile = new NativeArray<byte>(CellCount, allocator);
            Blocked = new NativeArray<byte>(CellCount, allocator);
            Clearance = new NativeArray<byte>(CellCount, allocator);
            Fit = new NativeArray<byte>(CellCount, allocator);

            var slots = NavField.Count * ClearanceClasses;

            Integration = new NativeArray<int>(slots * CellCount, allocator);
            FlowDir = new NativeArray<byte>(slots * CellCount, allocator);

            EnemyGoals = new NativeArray<int>(CellCount, allocator);
            DefenseGoals = new NativeArray<int>(CellCount, allocator);

            // Lazy-deletion heap: a cell is pushed at most once per incoming edge.
            Heap = new NativeArray<long>((CellCount * 8) + 16, allocator);

            BucketSize = cellSize * BucketCells;
            BucketsX = (width + BucketCells - 1) / BucketCells;
            BucketsY = (height + BucketCells - 1) / BucketCells;
            BucketCount = BucketsX * BucketsY;

            var maxItems = maxUnits + StaticItemsReserve;

            BucketStart = new NativeArray<int>(BucketCount + 1, allocator);
            BucketFill = new NativeArray<int>(BucketCount, allocator);
            Entries = new NativeArray<int>((maxItems * EntriesPerItem) + StaticEntriesReserve, allocator);
            ItemId = new NativeArray<int>(maxItems, allocator);
            ItemRange = new NativeArray<BucketRange>(maxItems, allocator);
            ItemStamp = new NativeArray<int>(maxItems, allocator);
            QueryResults = new NativeArray<int>(maxItems, allocator);
            ItemSlot = new NativeArray<int>(maxItems, allocator);
            QuerySlots = new NativeArray<int>(maxItems, allocator);

            UnitAlive = new NativeArray<byte>(maxUnits, allocator);
            UnitEntityId = new NativeArray<int>(maxUnits, allocator);
            UnitSeen = new NativeArray<int>(maxUnits, allocator);
            UnitPos = new NativeArray<Vector2>(maxUnits, allocator);
            UnitPrevPos = new NativeArray<Vector2>(maxUnits, allocator);
            UnitVel = new NativeArray<Vector2>(maxUnits, allocator);
            UnitGoal = new NativeArray<Vector2>(maxUnits, allocator);
            UnitViewOffset = new NativeArray<Vector2>(maxUnits, allocator);
            UnitRadius = new NativeArray<float>(maxUnits, allocator);
            UnitWallRadius = new NativeArray<float>(maxUnits, allocator);
            UnitSpeed = new NativeArray<float>(maxUnits, allocator);
            UnitSideTimer = new NativeArray<float>(maxUnits, allocator);
            UnitStuckTimer = new NativeArray<float>(maxUnits, allocator);
            UnitPauseTimer = new NativeArray<float>(maxUnits, allocator);
            UnitDetourTimer = new NativeArray<float>(maxUnits, allocator);
            UnitIdleTimer = new NativeArray<float>(maxUnits, allocator);
            UnitFlipTimer = new NativeArray<float>(maxUnits, allocator);
            UnitMode = new NativeArray<byte>(maxUnits, allocator);
            UnitField = new NativeArray<byte>(maxUnits, allocator);
            UnitClearance = new NativeArray<byte>(maxUnits, allocator);
            UnitMoving = new NativeArray<byte>(maxUnits, allocator);
            UnitImmovable = new NativeArray<byte>(maxUnits, allocator);
            UnitSide = new NativeArray<sbyte>(maxUnits, allocator);
            UnitRethinks = new NativeArray<byte>(maxUnits, allocator);
            FreeSlots = new NativeArray<int>(maxUnits, allocator);
            Congestion = new NativeArray<float>(CellCount, allocator);

            // Stack of free slots, lowest index on top so live units stay packed at the start of the arrays.
            for (var i = 0; i < maxUnits; i++)
                FreeSlots[i] = maxUnits - 1 - i;

            FreeCount = maxUnits;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            Tile.Dispose();
            Blocked.Dispose();
            Clearance.Dispose();
            Fit.Dispose();
            Integration.Dispose();
            FlowDir.Dispose();
            EnemyGoals.Dispose();
            DefenseGoals.Dispose();
            Heap.Dispose();
            BucketStart.Dispose();
            BucketFill.Dispose();
            Entries.Dispose();
            ItemId.Dispose();
            ItemRange.Dispose();
            ItemStamp.Dispose();
            QueryResults.Dispose();
            ItemSlot.Dispose();
            QuerySlots.Dispose();
            UnitAlive.Dispose();
            UnitEntityId.Dispose();
            UnitSeen.Dispose();
            UnitPos.Dispose();
            UnitPrevPos.Dispose();
            UnitVel.Dispose();
            UnitGoal.Dispose();
            UnitViewOffset.Dispose();
            UnitRadius.Dispose();
            UnitWallRadius.Dispose();
            UnitSpeed.Dispose();
            UnitSideTimer.Dispose();
            UnitStuckTimer.Dispose();
            UnitPauseTimer.Dispose();
            UnitDetourTimer.Dispose();
            UnitIdleTimer.Dispose();
            UnitFlipTimer.Dispose();
            UnitMode.Dispose();
            UnitField.Dispose();
            UnitClearance.Dispose();
            UnitMoving.Dispose();
            UnitImmovable.Dispose();
            UnitSide.Dispose();
            UnitRethinks.Dispose();
            FreeSlots.Dispose();
            Congestion.Dispose();
        }

        public bool IsDisposed => _disposed;

        // ---- Unit slots ----

        /// <summary>Takes a free slot, or returns -1 when the level budget (MaxUnits) is used up.</summary>
        public int AcquireSlot()
        {
            if (FreeCount == 0)
                return -1;

            var slot = FreeSlots[--FreeCount];

            UnitAlive[slot] = 1;
            UnitVel[slot] = Vector2.zero;
            UnitSideTimer[slot] = 0f;
            UnitStuckTimer[slot] = 0f;
            UnitPauseTimer[slot] = 0f;
            UnitDetourTimer[slot] = 0f;
            UnitIdleTimer[slot] = 0f;
            UnitFlipTimer[slot] = 0f;
            UnitMode[slot] = UnitModes.Hold;
            UnitMoving[slot] = 0;
            UnitImmovable[slot] = 0;
            UnitSide[slot] = 0;
            UnitRethinks[slot] = 0;
            UnitSeen[slot] = SyncStamp;

            if (slot >= SlotLimit)
                SlotLimit = slot + 1;

            return slot;
        }

        public void ReleaseSlot(int slot)
        {
            if (UnitAlive[slot] == 0)
                return;

            UnitAlive[slot] = 0;
            FreeSlots[FreeCount++] = slot;

            while (SlotLimit > 0 && UnitAlive[SlotLimit - 1] == 0)
                SlotLimit--;
        }

        // ---- Cells ----

        public bool InBounds(int cellX, int cellY)
        {
            var x = cellX - OriginX;
            var y = cellY - OriginY;

            return x >= 0 && y >= 0 && x < Width && y < Height;
        }

        /// <summary>Cell index for tilemap cell coordinates, or -1 outside the grid.</summary>
        public int IndexOf(int cellX, int cellY)
        {
            var x = cellX - OriginX;
            var y = cellY - OriginY;

            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return -1;

            return x + (y * Width);
        }

        /// <summary>Cell index under a world position, or -1 outside the grid.</summary>
        public int IndexAt(float worldX, float worldY)
        {
            var x = Mathf.FloorToInt((worldX - WorldMin.x) / CellSize);
            var y = Mathf.FloorToInt((worldY - WorldMin.y) / CellSize);

            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return -1;

            return x + (y * Width);
        }

        public Vector2 CellCenter(int index)
        {
            var x = index % Width;
            var y = index / Width;

            return new Vector2(
                WorldMin.x + ((x + 0.5f) * CellSize),
                WorldMin.y + ((y + 0.5f) * CellSize));
        }

        public Vector2 CellMin(int cellX, int cellY)
        {
            return new Vector2(
                WorldMin.x + ((cellX - OriginX) * CellSize),
                WorldMin.y + ((cellY - OriginY) * CellSize));
        }

        public bool IsWalkable(int index) => index >= 0 && Clearance[index] > 0;

        public static int FieldSlot(int field, int clearanceClass) =>
            (field * ClearanceClasses) + (clearanceClass - 1);

        // ---- Spatial grid ----

        public void GridBegin()
        {
            ItemCount = 0;
            EntryCount = 0;
        }

        /// <summary>
        /// Registers an item covering a world-space rectangle. Returns false when the level budget is exhausted;
        /// the item is then simply not visible to queries this tick.
        /// </summary>
        public bool GridAdd(int id, int slot, float minX, float minY, float maxX, float maxY)
        {
            if (ItemCount >= ItemId.Length)
                return false;

            var range = ToBucketRange(minX, minY, maxX, maxY);
            var entries = (range.MaxX - range.MinX + 1) * (range.MaxY - range.MinY + 1);

            if (EntryCount + entries > Entries.Length)
                return false;

            ItemId[ItemCount] = id;
            ItemSlot[ItemCount] = slot;
            ItemRange[ItemCount] = range;
            ItemStamp[ItemCount] = 0;

            ItemCount++;
            EntryCount += entries;

            return true;
        }

        public void GridBuild()
        {
            for (var b = 0; b < BucketCount; b++)
                BucketFill[b] = 0;

            for (var i = 0; i < ItemCount; i++)
            {
                var range = ItemRange[i];

                for (var y = range.MinY; y <= range.MaxY; y++)
                {
                    for (var x = range.MinX; x <= range.MaxX; x++)
                        BucketFill[x + (y * BucketsX)]++;
                }
            }

            var total = 0;

            for (var b = 0; b < BucketCount; b++)
            {
                BucketStart[b] = total;
                total += BucketFill[b];
                BucketFill[b] = 0;
            }

            BucketStart[BucketCount] = total;

            for (var i = 0; i < ItemCount; i++)
            {
                var range = ItemRange[i];

                for (var y = range.MinY; y <= range.MaxY; y++)
                {
                    for (var x = range.MinX; x <= range.MaxX; x++)
                    {
                        var bucket = x + (y * BucketsX);

                        Entries[BucketStart[bucket] + BucketFill[bucket]] = i;
                        BucketFill[bucket]++;
                    }
                }
            }

            _queryStamp = 0;
        }

        /// <summary>
        /// Collects ids of every item whose buckets touch the rectangle into <see cref="QueryResults"/> (and its
        /// sim slot, or -1 for a building, into <see cref="QuerySlots"/>), without duplicates. The result is a superset: callers do their own exact distance test.
        /// The buffer is reused by the next query.
        /// </summary>
        public int Query(float minX, float minY, float maxX, float maxY)
        {
            var range = ToBucketRange(minX, minY, maxX, maxY);
            var count = 0;

            _queryStamp++;

            for (var y = range.MinY; y <= range.MaxY; y++)
            {
                for (var x = range.MinX; x <= range.MaxX; x++)
                {
                    var bucket = x + (y * BucketsX);
                    var end = BucketStart[bucket + 1];

                    for (var e = BucketStart[bucket]; e < end; e++)
                    {
                        var item = Entries[e];

                        if (ItemStamp[item] == _queryStamp)
                            continue;

                        ItemStamp[item] = _queryStamp;
                        QueryResults[count] = ItemId[item];
                        QuerySlots[count] = ItemSlot[item];
                        count++;
                    }
                }
            }

            return count;
        }

        private BucketRange ToBucketRange(float minX, float minY, float maxX, float maxY)
        {
            return new BucketRange
            {
                MinX = (short)Mathf.Clamp(Mathf.FloorToInt((minX - WorldMin.x) / BucketSize), 0, BucketsX - 1),
                MinY = (short)Mathf.Clamp(Mathf.FloorToInt((minY - WorldMin.y) / BucketSize), 0, BucketsY - 1),
                MaxX = (short)Mathf.Clamp(Mathf.FloorToInt((maxX - WorldMin.x) / BucketSize), 0, BucketsX - 1),
                MaxY = (short)Mathf.Clamp(Mathf.FloorToInt((maxY - WorldMin.y) / BucketSize), 0, BucketsY - 1)
            };
        }
    }
}
