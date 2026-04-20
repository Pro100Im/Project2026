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

                        foreach (var cell in spawns.spawnMap.Value)
                        {
                            if (!map.tilemapMovement.Value.TryGetValue(cell, out var walkable) || !walkable)
                                continue;

                            if (map.occupancyMap.Value.ContainsKey(cell))
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
                            enemySpawn.AddSpawnPosition(chosenCell);
                        }
                    }
                }
            }
        }
    }
}