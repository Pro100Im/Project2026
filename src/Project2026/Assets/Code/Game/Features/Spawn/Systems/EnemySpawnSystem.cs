using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Spawn.Systems
{
    public class EnemySpawnSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _enemiesToSpawn;

        private readonly List<GameEntity> _buffer = new(32);

        public EnemySpawnSystem(GameContext gameContext)
        {
            _enemiesToSpawn = gameContext.GetGroup(GameMatcher
              .AllOf(
              GameMatcher.SpawnRequsted,
              GameMatcher.SpawnPosition,
              GameMatcher.EntityConfig,
              GameMatcher.GateNumber,
              GameMatcher.Enemy));
        }

        public void Execute()
        {
            foreach (var enemySpawn in _enemiesToSpawn.GetEntities(_buffer))
            {
                var entity = CreateGameEntity.Empty();
                entity.AddSpawnPosition(enemySpawn.spawnPosition.Value);
                entity.AddGateNumber(enemySpawn.gateNumber.Value);

                foreach(var property in enemySpawn.entityConfig.Value.Properties)
                    property.Apply(entity);

                enemySpawn.isDestructed = true;
            }
        }
    }
}