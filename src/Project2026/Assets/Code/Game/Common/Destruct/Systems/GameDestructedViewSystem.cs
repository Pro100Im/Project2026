using Code.Infrastructure.View;
using Code.Infrastructure.View.Pool;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Common.Destruct.Systems
{
    public class GameDestructedViewSystem : ICleanupSystem
    {
        private readonly IEntityViewPool _pool;
        private readonly IGroup<GameEntity> _entities;

        private readonly List<GameEntity> _entitiesBuffer = new(64);

        public GameDestructedViewSystem(IEntityViewPool pool)
        {
            _pool = pool;

            _entities = Contexts.sharedInstance.game.GetGroup(
              GameMatcher.AllOf(
                GameMatcher.Destructed,
                GameMatcher.View));
        }

        public void Cleanup()
        {
            var entities = _entities.GetEntities(_entitiesBuffer);

            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];

                entity.view.Value.ReleaseEntity();

                _pool.Release((EntityBehaviour)entity.view.Value);
            }
        }
    }
}
