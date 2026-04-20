using Entitas;
using UnityEngine;

namespace Code.Game.Features.Spawn.Systems
{
    public class EnemySelectSpawnPosSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _maps;
        private readonly IGroup<GameEntity> _enemies;
        private readonly IGroup<GameEntity> _spawnMaps;

        public EnemySelectSpawnPosSystem(GameContext gameContext)
        {
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
            foreach(var map in _maps)
            {
                foreach (var spawns in _spawnMaps)
                {
                    foreach (var enemySpawn in _enemies)
                    {
                        foreach (var cell in spawns.spawnMap.Value)
                        {
                            if (!map.tilemapMovement.Value.TryGetValue(cell, out var walkable) || !walkable)
                                continue;

                            if (map.occupancyMap.Value.ContainsKey(cell))
                                continue;

                            var position = new Vector3(cell.x, cell.y);

                            if (!enemySpawn.hasSpawnPosition)
                                enemySpawn.AddSpawnPosition(position);

                            //map.occupancyMap.Value.Add(cell, enemySpawn.id.Value);
                        }        
                    }
                }
            }
        }
    }
}