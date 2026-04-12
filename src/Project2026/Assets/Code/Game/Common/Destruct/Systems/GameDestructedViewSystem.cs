using Entitas;
using UnityEngine;

namespace Code.Game.Common.Destruct.Systems
{
    public class GameDestructedViewSystem : ICleanupSystem
    {
        private readonly IGroup<GameEntity> _entities;

        public GameDestructedViewSystem(GameContext game) =>
          _entities = game.GetGroup(
            GameMatcher.AllOf(
              GameMatcher.Destructed,
              GameMatcher.View));

        public void Cleanup()
        {
            foreach (GameEntity entity in _entities)
            {
                entity.view.Value.ReleaseEntity();

                Object.Destroy(entity.view.Value.GameObject);
            }
        }
    }
}