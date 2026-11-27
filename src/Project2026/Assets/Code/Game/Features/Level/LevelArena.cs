using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
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
    /// All per-level simulation memory, cut out of one native block that is allocated when the level map binds
    /// and freed when it unbinds. Nothing in here grows or reallocates while the level runs: when a budget runs
    /// out the request is dropped and counted in <see cref="Overflows"/>.
    /// Data only: systems own the logic that fills and reads these arrays.
    /// </summary>
    public sealed class LevelArena : IDisposable
    {
        public const int Unreachable = int.MaxValue;
        public const int ClearanceClasses = 3;
        public const byte NoDirection = 255;
        public const int StaticItemsReserve = 64;

        private const int BucketCells = 2;

        // A body up to 3 cells wide spans up to 3 x 3 buckets of 2 cells.
        private const int EntriesPerItem = 9;
        private const int StaticEntriesReserve = 512;
        private const int Alignment = 16;

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
        public readonly int MaxItems;

        /// <summary>Size of the single native block that holds every array below.</summary>
        public readonly int TotalBytes;

        /// <summary>How many times a fixed budget ran out this level. Should stay 0.</summary>
        public int Overflows;

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

        // Cell claims of the targeting layer: entity id per cell, 0 = none (entity ids start above 0).
        // Occupant: footprint of every unit and building, rebuilt every simulation step.
        // SurroundOwner: attack slot claimed by a unit.
        // SpawnReserved: cells taken by spawns this frame; a cell is reserved while it holds SpawnStamp.
        public NativeArray<int> Occupant;
        public NativeArray<int> SurroundOwner;
        public NativeArray<int> SpawnReserved;
        public int SpawnStamp = 1;

        private NativeArray<byte> _block;
        private int _cursor;
        private bool _measuring;
        private int _spawnFrame = -1;
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
            MaxItems = maxUnits + StaticItemsReserve;

            BucketSize = cellSize * BucketCells;
            BucketsX = (width + BucketCells - 1) / BucketCells;
            BucketsY = (height + BucketCells - 1) / BucketCells;
            BucketCount = BucketsX * BucketsY;

            // Two passes over the same layout: the first one only measures, the second one cuts the views out of
            // a single allocation. One block per level is one malloc and one free, so levels do not fragment the
            // native heap.
            _measuring = true;
            Layout();

            TotalBytes = _cursor;
            _block = new NativeArray<byte>(Mathf.Max(TotalBytes, Alignment), Allocator.Persistent);

            _measuring = false;
            Layout();

            // Stack of free slots, lowest index on top so live units stay packed at the start of the arrays.
            for (var i = 0; i < maxUnits; i++)
                FreeSlots[i] = maxUnits - 1 - i;

            FreeCount = maxUnits;
        }

        private void Layout()
        {
            _cursor = 0;

            var slots = NavField.Count * ClearanceClasses;

            Tile = Take<byte>(CellCount);
            Blocked = Take<byte>(CellCount);
            Clearance = Take<byte>(CellCount);
            Fit = Take<byte>(CellCount);

            Integration = Take<int>(slots * CellCount);
            FlowDir = Take<byte>(slots * CellCount);

            EnemyGoals = Take<int>(CellCount);
            DefenseGoals = Take<int>(CellCount);

            // Lazy-deletion heap: a cell is pushed at most once per incoming edge.
            Heap = Take<long>((CellCount * 8) + 16);

            Congestion = Take<float>(CellCount);
            Occupant = Take<int>(CellCount);
            SurroundOwner = Take<int>(CellCount);
            SpawnReserved = Take<int>(CellCount);

            BucketStart = Take<int>(BucketCount + 1);
            BucketFill = Take<int>(BucketCount);
            Entries = Take<int>((MaxItems * EntriesPerItem) + StaticEntriesReserve);
            ItemId = Take<int>(MaxItems);
            ItemRange = Take<BucketRange>(MaxItems);
            ItemStamp = Take<int>(MaxItems);
            ItemSlot = Take<int>(MaxItems);
            QueryResults = Take<int>(MaxItems);
            QuerySlots = Take<int>(MaxItems);

            UnitAlive = Take<byte>(MaxUnits);
            UnitEntityId = Take<int>(MaxUnits);
            UnitSeen = Take<int>(MaxUnits);
            UnitPos = Take<Vector2>(MaxUnits);
            UnitPrevPos = Take<Vector2>(MaxUnits);
            UnitVel = Take<Vector2>(MaxUnits);
            UnitGoal = Take<Vector2>(MaxUnits);
            UnitViewOffset = Take<Vector2>(MaxUnits);
            UnitRadius = Take<float>(MaxUnits);
            UnitWallRadius = Take<float>(MaxUnits);
            UnitSpeed = Take<float>(MaxUnits);
            UnitSideTimer = Take<float>(MaxUnits);
            UnitStuckTimer = Take<float>(MaxUnits);
            UnitPauseTimer = Take<float>(MaxUnits);
            UnitDetourTimer = Take<float>(MaxUnits);
            UnitIdleTimer = Take<float>(MaxUnits);
            UnitFlipTimer = Take<float>(MaxUnits);
            UnitMode = Take<byte>(MaxUnits);
            UnitField = Take<byte>(MaxUnits);
            UnitClearance = Take<byte>(MaxUnits);
            UnitMoving = Take<byte>(MaxUnits);
            UnitImmovable = Take<byte>(MaxUnits);
            UnitSide = Take<sbyte>(MaxUnits);
            UnitRethinks = Take<byte>(MaxUnits);
            FreeSlots = Take<int>(MaxUnits);
        }

        /// <summary>Next aligned view into the block. While measuring it only advances the cursor.</summary>
        private NativeArray<T> Take<T>(int length) where T : struct
        {
            var bytes = UnsafeUtility.SizeOf<T>() * length;
            var offset = (_cursor + Alignment - 1) & ~(Alignment - 1);

            _cursor = offset + bytes;

            return _measuring ? default : _block.GetSubArray(offset, bytes).Reinterpret<T>(1);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            // The views share the block, so only the block itself is freed. A view used after this throws
            // in the editor instead of reading freed memory.
            if (_block.IsCreated)
                _block.Dispose();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Overflows > 0)
                Debug.LogWarning($"[LevelArena] Level budget ran out {Overflows} times. Raise LevelConfig.MaxUnits.");
#endif
        }

        public bool IsDisposed => _disposed;

        /// <summary>A fixed budget ran out (unit slots, spatial grid, heap): the request is dropped, never grown.</summary>
        public void NoteOverflow()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Overflows == 0)
                Debug.LogWarning("[LevelArena] Level budget exceeded, requests are dropped. Raise LevelConfig.MaxUnits.");
#endif
            Overflows++;
        }

        // ---- Unit slots ----

        /// <summary>Takes a free slot, or returns -1 when the level budget (MaxUnits) is used up.</summary>
        public int AcquireSlot()
        {
            if (FreeCount == 0)
            {
                NoteOverflow();
                return -1;
            }

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

        /// <summary>Tilemap cell coordinates of a cell index.</summary>
        public Vector3Int CellOf(int index, int z = 0) =>
            new Vector3Int(OriginX + (index % Width), OriginY + (index / Width), z);

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

        /// <summary>True for a cell of the level tilemap; buildings standing on it do not matter here.</summary>
        public bool IsTile(Vector3Int cell)
        {
            var index = IndexOf(cell.x, cell.y);

            return index >= 0 && Tile[index] != 0;
        }

        /// <summary>World centre of a tilemap cell; false for a cell that is not part of the level tilemap.</summary>
        public bool TryGetCellWorld(Vector3Int cell, out Vector2 center)
        {
            var index = IndexOf(cell.x, cell.y);

            if (index < 0 || Tile[index] == 0)
            {
                center = default;
                return false;
            }

            center = CellCenter(index);
            return true;
        }

        public static int FieldSlot(int field, int clearanceClass) =>
            (field * ClearanceClasses) + (clearanceClass - 1);

        // ---- Cell claims ----

        public int OccupantAt(Vector3Int cell)
        {
            var index = IndexOf(cell.x, cell.y);

            return index >= 0 ? Occupant[index] : 0;
        }

        public int SurroundOwnerAt(Vector3Int cell)
        {
            var index = IndexOf(cell.x, cell.y);

            return index >= 0 ? SurroundOwner[index] : 0;
        }

        public void ClaimSurround(Vector3Int cell, int unitId)
        {
            var index = IndexOf(cell.x, cell.y);

            if (index >= 0)
                SurroundOwner[index] = unitId;
        }

        /// <summary>Frees a slot only while it is still held by this unit.</summary>
        public void ReleaseSurround(Vector3Int cell, int unitId)
        {
            var index = IndexOf(cell.x, cell.y);

            if (index >= 0 && SurroundOwner[index] == unitId)
                SurroundOwner[index] = 0;
        }

        public void ClearSurround(Vector3Int cell)
        {
            var index = IndexOf(cell.x, cell.y);

            if (index >= 0)
                SurroundOwner[index] = 0;
        }

        /// <summary>Spawn reservations live for one frame: the first spawn system of a frame drops the old ones.</summary>
        public void BeginSpawnFrame(int frame)
        {
            if (frame == _spawnFrame)
                return;

            _spawnFrame = frame;
            SpawnStamp++;
        }

        public bool IsSpawnReserved(Vector3Int cell)
        {
            var index = IndexOf(cell.x, cell.y);

            return index >= 0 && SpawnReserved[index] == SpawnStamp;
        }

        public void ReserveSpawn(Vector3Int cell)
        {
            var index = IndexOf(cell.x, cell.y);

            if (index >= 0)
                SpawnReserved[index] = SpawnStamp;
        }

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
            {
                NoteOverflow();
                return false;
            }

            var range = ToBucketRange(minX, minY, maxX, maxY);
            var entries = (range.MaxX - range.MinX + 1) * (range.MaxY - range.MinY + 1);

            if (EntryCount + entries > Entries.Length)
            {
                NoteOverflow();
                return false;
            }

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
