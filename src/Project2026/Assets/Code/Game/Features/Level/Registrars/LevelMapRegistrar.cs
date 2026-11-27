using Code.Game.StaticData.Configs;
using Code.Infrastructure.View.Registrars;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Game.Features.Level.Registrars
{
    /// <summary>
    /// Binds the level map: reads the tilemaps once and builds the <see cref="LevelArena"/>, the only per-level
    /// simulation memory. The arena is freed when the map unbinds.
    /// </summary>
    public class LevelMapRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private Tilemap _tilemap;
        [Space]
        [SerializeField] private Tilemap _flowTargets;
        [SerializeField] private Tilemap _defenseTargets;

        [Header("Debug gizmos (play mode, object selected)")]
        [SerializeField] private bool _drawArena;
        [SerializeField, Range(0, NavField.Count - 1)] private int _debugField;
        [SerializeField, Range(1, LevelArena.ClearanceClasses)] private int _debugClearance = 1;

        private LevelArena _arena;

        public override void RegisterComponents()
        {
            DisposeArena();

            _arena = CreateArena();
            Entity.AddArena(_arena);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[LevelArena] {_arena.Width}x{_arena.Height} cells, {_arena.MaxUnits} units: " +
                $"{_arena.TotalBytes / 1024} KB in one block.");
#endif
        }

        public override void UnregisterComponents()
        {
            if (Entity.hasArena)
            {
                // Removed components go back to the Entitas pool: do not let the pooled one keep the arena alive.
                Entity.arena.Value = null;
                Entity.RemoveArena();
            }

            DisposeArena();
        }

        private void OnDestroy() => DisposeArena();

        private void DisposeArena()
        {
            _arena?.Dispose();
            _arena = null;
        }

        /// <summary>
        /// Allocates every per-level simulation array once. The grid covers the walkable tilemap and both goal
        /// tilemaps plus a one-cell wall border, so a walkable cell always has all 8 neighbours inside the grid.
        /// </summary>
        private LevelArena CreateArena()
        {
            const int padding = 1;

            var bounds = _tilemap.cellBounds;
            var minX = bounds.xMin;
            var minY = bounds.yMin;
            var maxX = bounds.xMax;
            var maxY = bounds.yMax;

            Encapsulate(_flowTargets, ref minX, ref minY, ref maxX, ref maxY);
            Encapsulate(_defenseTargets, ref minX, ref minY, ref maxX, ref maxY);

            var origin = new Vector3Int(minX, minY, 0);
            var originCenter = _tilemap.GetCellCenterWorld(origin);
            var cellSize = Mathf.Abs(_tilemap.GetCellCenterWorld(origin + Vector3Int.right).x - originCenter.x);
            var border = cellSize * (0.5f + padding);

            var arena = new LevelArena(
                minX - padding,
                minY - padding,
                Mathf.Max(maxX - minX, 1) + (padding * 2),
                Mathf.Max(maxY - minY, 1) + (padding * 2),
                cellSize,
                new Vector2(originCenter.x - border, originCenter.y - border),
                ResolveMaxUnits());

            foreach (var cell in bounds.allPositionsWithin)
            {
                if (_tilemap.GetTile(cell) == null)
                    continue;

                var index = arena.IndexOf(cell.x, cell.y);

                if (index >= 0)
                    arena.Tile[index] = 1;
            }

            arena.EnemyGoalCount = CopyGoals(arena, _flowTargets, arena.EnemyGoals);
            arena.DefenseGoalCount = CopyGoals(arena, _defenseTargets, arena.DefenseGoals);

            return arena;
        }

        private static int CopyGoals(LevelArena arena, Tilemap tilemap, NativeArray<int> goals)
        {
            if (tilemap == null)
                return 0;

            var count = 0;

            foreach (var cell in tilemap.cellBounds.allPositionsWithin)
            {
                if (tilemap.GetTile(cell) == null)
                    continue;

                var index = arena.IndexOf(cell.x, cell.y);

                if (index >= 0 && count < goals.Length)
                    goals[count++] = index;
            }

            return count;
        }

        private static void Encapsulate(Tilemap tilemap, ref int minX, ref int minY, ref int maxX, ref int maxY)
        {
            if (tilemap == null)
                return;

            foreach (var cell in tilemap.cellBounds.allPositionsWithin)
            {
                if (tilemap.GetTile(cell) == null)
                    continue;

                minX = Mathf.Min(minX, cell.x);
                minY = Mathf.Min(minY, cell.y);
                maxX = Mathf.Max(maxX, cell.x + 1);
                maxY = Mathf.Max(maxY, cell.y + 1);
            }
        }

        private static int ResolveMaxUnits()
        {
            var session = Contexts.sharedInstance.game.GetGroup(GameMatcher.ActiveLevel).GetSingleEntity();

            return session != null && session.activeLevel.Value != null
                ? Mathf.Max(session.activeLevel.Value.MaxUnits, 16)
                : LevelConfig.DefaultMaxUnits;
        }

        private void OnDrawGizmosSelected()
        {
            DrawTargetGizmos(_flowTargets, Color.red);
            DrawTargetGizmos(_defenseTargets, Color.blue);

            if (_drawArena && _arena != null && !_arena.IsDisposed)
                DrawArenaGizmos(_arena);
        }

        /// <summary>
        /// Red = building, orange = too narrow for the chosen clearance, magenta = no path to the goal,
        /// green = goal, cyan = flow direction.
        /// </summary>
        private void DrawArenaGizmos(LevelArena arena)
        {
            var offset = LevelArena.FieldSlot(_debugField, _debugClearance) * arena.CellCount;
            var cube = new Vector3(arena.CellSize, arena.CellSize, 0f) * 0.9f;

            for (var cell = 0; cell < arena.CellCount; cell++)
            {
                var center = (Vector3)arena.CellCenter(cell);

                if (arena.Blocked[cell] != 0)
                {
                    Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
                    Gizmos.DrawCube(center, cube);
                    continue;
                }

                if (arena.Clearance[cell] == 0)
                    continue;

                if (arena.Fit[cell] < _debugClearance)
                {
                    Gizmos.color = new Color(1f, 0.6f, 0f, 0.35f);
                    Gizmos.DrawCube(center, cube);
                    continue;
                }

                var cost = arena.Integration[offset + cell];

                if (cost == LevelArena.Unreachable)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireCube(center, cube);
                    continue;
                }

                var dir = arena.FlowDir[offset + cell];

                if (dir == LevelArena.NoDirection)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(center, cube * 0.5f);
                    continue;
                }

                var step = new Vector3(LevelArena.DirX[dir], LevelArena.DirY[dir], 0f).normalized
                    * (arena.CellSize * 0.45f);

                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(center - (step * 0.5f), center + step);
                Gizmos.DrawSphere(center + step, arena.CellSize * 0.06f);
            }
        }

        private static void DrawTargetGizmos(Tilemap tilemap, Color color)
        {
            if (tilemap == null)
                return;

            Gizmos.color = color;
            var bounds = tilemap.cellBounds;

            foreach (var pos in bounds.allPositionsWithin)
            {
                if (tilemap.GetTile(pos) == null)
                    continue;

                var cell = tilemap.GetCellCenterWorld(pos);
                Gizmos.DrawWireCube(cell, tilemap.cellSize);
            }
        }
    }
}
