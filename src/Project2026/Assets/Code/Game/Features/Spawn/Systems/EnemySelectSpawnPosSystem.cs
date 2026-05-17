using Code.Game.Common.Random;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Spawn.Systems
{
    public class EnemySelectSpawnPosSystem : IExecuteSystem
    {
        private readonly IRandomService _random;
        private readonly IGroup<GameEntity> _enemies;
        private readonly IGroup<GameEntity> _spawnMaps;
        private readonly IGroup<GameEntity> _maps;

        private readonly List<GameEntity> _spawnMapsBuffer = new(8);
        private readonly List<GameEntity> _enemiesBuffer = new(124);

        public EnemySelectSpawnPosSystem(GameContext context, IRandomService random)
        {
            _random = random;

            _enemies = context.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.SpawnRequsted,
                    GameMatcher.Enemy,
                    GameMatcher.UnitSize));

            _spawnMaps = context.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.SpawnMap,
                    GameMatcher.Enemy));

            _maps = context.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.OccupField,
                    GameMatcher.ReservedField,
                    GameMatcher.SpawnReservedField,
                    GameMatcher.TilemapMovement));
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();

            if (map == null)
                return;

            var occupField = map.occupField.Value;
            var reservedField = map.reservedField.Value;
            var spawnReservedField = map.spawnReservedField.Value;
            var tilemap = map.tilemapMovement.Value;
            var spawnMaps = _spawnMaps.GetEntities(_spawnMapsBuffer);

            spawnReservedField.Clear();

            for ( var i = 0; i < spawnMaps.Count; i++)
            {
                var spawnMap = spawnMaps[i];
                var points = spawnMap.spawnMap.Value;
                var enemies = _enemies.GetEntities(_enemiesBuffer);

                for (var j = 0; j < enemies.Count; j++)
                {
                    var enemy = enemies[j];

                    if (enemy.hasSpawnPosition) 
                        continue;

                    var size = enemy.unitSize.Value;
                    var count = 0;
                    var chosenPos = Vector3.zero;
                    var chosenCell = Vector3Int.zero;
                    bool found = false;

                    foreach (var kvp in points)
                    {
                        var cell = kvp.Key;
                        var worldPos = kvp.Value;

                        if (!CanFit(cell, size, occupField, reservedField, spawnReservedField, tilemap))
                            continue;

                        count++;

                        if (_random.GetGlobalRandom(0, count) == 0)
                        {
                            chosenPos = worldPos;
                            chosenCell = cell;
                            found = true;
                        }
                    }

                    if (found)
                    {
                        enemy.AddSpawnPosition(chosenPos);
                        enemy.AddCurrentCell(chosenCell);

                        ReserveCells(chosenCell, size, spawnReservedField);
                    }
                }
            }
        }

        private bool CanFit(Vector3Int origin, Vector2Int size, Dictionary<Vector3Int, int> occupied, Dictionary<Vector3Int, int> reserved,
                            HashSet<Vector3Int> spawnReserved, Dictionary<Vector3Int, Vector3> tilemap)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    var checkCell = new Vector3Int(origin.x + x, origin.y + y, origin.z);

                    if (!tilemap.ContainsKey(checkCell)) 
                        return false;

                    if (occupied.ContainsKey(checkCell)) 
                        return false;

                    if (reserved.ContainsKey(checkCell)) 
                        return false;

                    if (spawnReserved.Contains(checkCell)) 
                        return false;
                }
            }

            return true;
        }

        private void ReserveCells(Vector3Int origin, Vector2Int size, HashSet<Vector3Int> reserved)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    reserved.Add(new Vector3Int(origin.x + x, origin.y + y, origin.z));
                }
            }
        }
    }
}