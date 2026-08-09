using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Spawn.Systems
{
    public class EnemySpawnSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _enemiesToSpawn;

        private readonly List<GameEntity> _buffer = new(124);

        public EnemySpawnSystem()
        {
            _enemiesToSpawn = Contexts.sharedInstance.game.GetGroup(GameMatcher
              .AllOf(
              GameMatcher.SpawnRequsted,
              GameMatcher.SpawnPosition,
              GameMatcher.CurrentCell,
              GameMatcher.EntityConfig,
              GameMatcher.Enemy));
        }

        public void Execute()
        {
            var enemiesToSpawn = _enemiesToSpawn.GetEntities(_buffer);

            for (var i = 0; i < enemiesToSpawn.Count; i++)
            {
                var enemySpawn = enemiesToSpawn[i];
                var entity = CreateGameEntity.Empty();

                entity.AddSpawnPosition(enemySpawn.spawnPosition.Value);
                entity.AddCurrentCell(enemySpawn.currentCell.Value);
                entity.isMovementAvailable = true;
                entity.isEnemy = true;

                foreach(var property in enemySpawn.entityConfig.Value.Properties)
                    property.Apply(entity);

                enemySpawn.isDestructed = true;
            }
        }
    }
}