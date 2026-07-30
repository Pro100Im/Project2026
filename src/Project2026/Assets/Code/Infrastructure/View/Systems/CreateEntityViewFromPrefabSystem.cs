using Code.Infrastructure.View;
using Code.Infrastructure.View.Pool;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Infrastructure.View.Systems
{
    public class CreateEntityViewFromPrefabSystem : IExecuteSystem
    {
        private readonly IEntityViewPool _pool;
        private readonly IGroup<GameEntity> _entities;
        private readonly List<GameEntity> _buffer = new(32);

        public CreateEntityViewFromPrefabSystem(GameContext game, IEntityViewPool pool)
        {
            _pool = pool;

            _entities = game.GetGroup(GameMatcher
              .AllOf
              (GameMatcher.ViewPrefab,
               GameMatcher.SpawnPosition)
              .NoneOf
              (GameMatcher.View));
        }

        public void Execute()
        {
            var entities = _entities.GetEntities(_buffer);

            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];

                var view = _pool.Get(entity.viewPrefab.Value, entity.spawnPosition.Value, Quaternion.identity);

                view.SetEntity(entity);
                view.gameObject.SetActive(true);

                entity.RemoveSpawnPosition();
                entity.RemoveViewPrefab();
            }
        }
    }
}
