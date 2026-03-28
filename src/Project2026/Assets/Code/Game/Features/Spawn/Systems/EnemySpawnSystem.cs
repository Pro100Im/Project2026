using Code.Game.Features.Enemy.Factory;
using Entitas;

namespace Code.Game.Features.Spawn.Systems
{
    public class EnemySpawnSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _enemiesToSpawn;
        private readonly EnemyFactory _enemyFactory;

        public EnemySpawnSystem(GameContext gameContext, EnemyFactory enemyFactory)
        {
            _enemiesToSpawn = gameContext.GetGroup(GameMatcher
              .AllOf(
              GameMatcher.SpawnRequsted,
              GameMatcher.SpawnPosition,
              GameMatcher.EntityConfig,
              GameMatcher.Enemy));

            _enemyFactory = enemyFactory;
        }

        public void Execute()
        {
            foreach (var enemy in _enemiesToSpawn)
            {
                _enemyFactory.Create(enemy.entityConfig.Value, enemy.spawnPosition.Value);
            }
        }
    }
}