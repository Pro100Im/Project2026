using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Common.Destruct.Systems
{
    public class GameDestructedViewSystem : ICleanupSystem
    {
        private readonly IGroup<GameEntity> _entities;

        private readonly List<GameEntity> _entitiesBuffer = new(64);

        public GameDestructedViewSystem(GameContext game) =>
          _entities = game.GetGroup(
            GameMatcher.AllOf(
              GameMatcher.Destructed,
              GameMatcher.View));

        public void Cleanup()
        {
            var entities = _entities.GetEntities(_entitiesBuffer);

            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];

                entity.view.Value.ReleaseEntity();

                Object.Destroy(entity.view.Value.GameObject);
            }
        }
    }
}