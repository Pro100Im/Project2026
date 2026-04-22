using Code.Game.Common.Random;
using Entitas;
using UnityEngine;

namespace Code.Game.Features.Spawn.Systems
{
    public class EnemySelectSpawnPosSystem : IExecuteSystem
    {
        private readonly IRandomService _randomService;

        private readonly IGroup<GameEntity> _maps;
        private readonly IGroup<GameEntity> _enemies;
        private readonly IGroup<GameEntity> _spawnMaps;

        public EnemySelectSpawnPosSystem(GameContext gameContext, IRandomService randomService)
        {
            _randomService = randomService;

            _maps = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.TilemapMovement,
                GameMatcher.GridSize));

            _enemies = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.SpawnRequsted,
                GameMatcher.EntityConfig,
                GameMatcher.UnitSize,
                GameMatcher.Enemy));

            _spawnMaps = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.SpawnMap,
                GameMatcher.Enemy));
        }

        public void Execute()
        {
            foreach (var map in _maps)
            {
                var gridSize = map.gridSize.Value;

                foreach (var spawns in _spawnMaps)
                {
                    foreach (var enemySpawn in _enemies)
                    {
                        var count = 0;
                        var chosenCell = new Vector3Int();
                        var chosenWorldPos = new Vector3();
                        var found = false;
                        var unitSize = enemySpawn.unitSize.Value;

                        foreach (var cell in spawns.spawnMap.Value)
                        {
                            var canSpawn = true;

                            for (int x = 0; x < unitSize.x; x++)
                            {
                                for (int y = 0; y < unitSize.y; y++)
                                {
                                    var checkCell = cell.Value + new Vector3(x * gridSize.x, y * gridSize.y);

                                    if (!map.tilemapMovement.Value.TryGetValue(cell.Key, out var walkable))
                                    {
                                        canSpawn = false;

                                        break;
                                    }

                                    if (map.occupancyMap.Value.ContainsKey(cell.Key))
                                    {
                                        canSpawn = false;

                                        break;
                                    }
                                }

                                if (!canSpawn)
                                    break;
                            }

                            if (!canSpawn)
                                continue;

                            count++;

                            if (_randomService.GetGlobalRandom(0, count) == 0)
                            {
                                chosenCell = cell.Key;
                                chosenWorldPos = cell.Value;
                                found = true;
                            }
                        }

                        if (found && !enemySpawn.hasSpawnPosition)
                        {
                            if (unitSize.x > 1 || unitSize.y > 1)
                            {
                                var offset = new Vector3((unitSize.x - 1) * gridSize.x * 0.5f, (unitSize.y - 1) * gridSize.y * 0.5f);

                                chosenWorldPos += offset;
                            }

                            enemySpawn.AddCurrentCell(chosenCell);
                            enemySpawn.AddSpawnPosition(chosenWorldPos);
                        }
                    }
                }
            }
        }
    }
}