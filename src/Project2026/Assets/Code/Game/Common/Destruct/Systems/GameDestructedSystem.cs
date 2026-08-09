using System.Collections.Generic;
using Entitas;

namespace Code.Game.Common.Destruct.Systems
{
    public class GameDestructedSystem : ICleanupSystem
    {
        private readonly IGroup<GameEntity> _entities;
        private readonly List<GameEntity> _entitiesBuffer = new(64);

        public GameDestructedSystem() =>
          _entities = Contexts.sharedInstance.game.GetGroup(GameMatcher.Destructed);

        public void Cleanup()
        {
            var entities = _entities.GetEntities(_entitiesBuffer);

            for (var i = 0; i < entities.Count; i++)
                entities[i].Destroy();
        }
    }
}