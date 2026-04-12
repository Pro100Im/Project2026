using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Infrastructure.View.Systems
{
    public class CreateEntityViewFromPrefabSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _entities;
        private readonly List<GameEntity> _buffer = new(32);

        public CreateEntityViewFromPrefabSystem(GameContext game)
        {
            _entities = game.GetGroup(GameMatcher
              .AllOf
              (GameMatcher.ViewPrefab,
               GameMatcher.SpawnPosition)
              .NoneOf
              (GameMatcher.View));
        }

        public void Execute()
        {
            foreach (GameEntity entity in _entities.GetEntities(_buffer))
            {
                var view = GameObject.Instantiate<EntityBehaviour>(entity.viewPrefab.Value, entity.spawnPosition.Value, Quaternion.identity, null);

                view.SetEntity(entity);

                entity.RemoveSpawnPosition();
                entity.RemoveViewPrefab();
            }
        }
    }
}