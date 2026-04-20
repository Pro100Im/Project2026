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

            _maps = gameContext.GetGroup(GameMatcher.TilemapMovement);
        }

        public void Execute()
        {
            foreach (var map in _maps)
            {
                foreach (var spawns in _spawnMaps)
                {
                    foreach (var enemySpawn in _enemies)
                    {
                        var count = 0;
                        var chosenCell = new Vector3();
                        var found = false;
                        var unitSize = enemySpawn.unitSize.Value;

                        foreach (var cell in spawns.spawnMap.Value)
                        {
                            var canSpawn = true;

                            for (int x = 0; x < unitSize; x++)
                            {
                                for (int y = 0; y < unitSize; y++)
                                {
                                    var checkCell = cell + new Vector3(x * 0.4f, y * 0.4f);

                                    if (!map.tilemapMovement.Value.TryGetValue(checkCell, out var walkable) || !walkable)
                                    {
                                        canSpawn = false;

                                        break;
                                    }

                                    if (map.occupancyMap.Value.ContainsKey(checkCell))
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
                                chosenCell = cell;
                                found = true;
                            }
                        }

                        if (found && !enemySpawn.hasSpawnPosition)
                        {
                            if (unitSize > 1)
                            {
                                var offset = new Vector3(
                                    (unitSize - 1) * 0.4f * 0.5f,
                                    (unitSize - 1) * 0.4f * 0.5f,
                                    0
                                );

                                chosenCell += offset;
                            }

                            enemySpawn.AddSpawnPosition(chosenCell);
                        }
                    }
                }
            }
        }
    }
}