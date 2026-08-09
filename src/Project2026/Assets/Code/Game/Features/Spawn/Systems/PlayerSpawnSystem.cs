using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Spawn.Systems
{
    public class PlayerSpawnSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _unitsToSpawn;

        private readonly List<GameEntity> _buffer = new(124);

        public PlayerSpawnSystem()
        {
            _unitsToSpawn = Contexts.sharedInstance.game.GetGroup(GameMatcher
              .AllOf(
              GameMatcher.SpawnRequsted,
              GameMatcher.SpawnPosition,
              GameMatcher.CurrentCell,
              GameMatcher.EntityConfig,
              GameMatcher.Player));
        }

        public void Execute()
        {
            var unitsToSpawn = _unitsToSpawn.GetEntities(_buffer);

            for (var i = 0; i < unitsToSpawn.Count; i++)
            {
                var unitSpawn = unitsToSpawn[i];
                var entity = CreateGameEntity.Empty();

                entity.AddSpawnPosition(unitSpawn.spawnPosition.Value);
                entity.AddCurrentCell(unitSpawn.currentCell.Value);
                entity.isMovementAvailable = true;
                entity.isPlayer = true;

                foreach (var property in unitSpawn.entityConfig.Value.Properties)
                    property.Apply(entity);

                unitSpawn.isDestructed = true;
            }
        }
    }
}