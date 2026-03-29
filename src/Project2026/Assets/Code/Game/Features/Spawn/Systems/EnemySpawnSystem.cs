using Code.Game.Features.Enemy.Factory;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Spawn.Systems
{
    public class EnemySpawnSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _enemiesToSpawn;
        private readonly EnemyFactory _enemyFactory;

        private readonly List<GameEntity> _buffer = new(32);

        public EnemySpawnSystem(GameContext gameContext, EnemyFactory enemyFactory)
        {
            _enemiesToSpawn = gameContext.GetGroup(GameMatcher
              .AllOf(
              GameMatcher.SpawnRequsted,
              GameMatcher.SpawnPosition,
              GameMatcher.EntityConfig,
              GameMatcher.SortOrder,
              GameMatcher.Enemy));

            _enemyFactory = enemyFactory;
        }

        public void Execute()
        {
            foreach (var enemySpawn in _enemiesToSpawn.GetEntities(_buffer))
            {
                _enemyFactory.Create(enemySpawn.entityConfig.Value, enemySpawn.spawnPosition.Value, enemySpawn.sortOrder.Value);

                enemySpawn.Destroy();
            }
        }
    }
}