using Code.Game.Features.Enemy.Factory;
using Entitas;
using UnityEngine;

namespace Code.Game.Features.Spawn.Systems
{
    public class EnemySelectSpawnPosSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _enemies;
        private readonly IGroup<GameEntity> _spawnPositions;

        public EnemySelectSpawnPosSystem(GameContext gameContext, EnemyFactory enemyFactory)
        {
            _enemies = gameContext.GetGroup(GameMatcher
              .AllOf(
              GameMatcher.SpawnRequsted,
              GameMatcher.EntityConfig,
              GameMatcher.Enemy));

            _spawnPositions = gameContext.GetGroup(GameMatcher
              .AllOf(
              GameMatcher.SpawnPositions,
              GameMatcher.SpawnPositionSortOrders,
              GameMatcher.SpawnPositionGates,
              GameMatcher.Enemy));
        }

        public void Execute()
        {
            foreach(var spawnPos in _spawnPositions)
            {
                foreach (var enemySpawn in _enemies)
                {
                    var spawnPositions = spawnPos.spawnPositions.Value;
                    var randomIndex = Random.Range(0, spawnPositions.Length);
                    var position = spawnPositions[randomIndex];
                    var order = spawnPos.spawnPositionSortOrders.Value[randomIndex];
                    var gate = spawnPos.spawnPositionGates.Value[randomIndex];

                    if (!enemySpawn.hasSpawnPosition)
                        enemySpawn.AddSpawnPosition(position);

                    if (!enemySpawn.hasSortOrder)
                        enemySpawn.AddSortOrder(order);

                    if (!enemySpawn.hasGateNumber)
                        enemySpawn.AddGateNumber(gate);
                }
            }
        }
    }
}