using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Input.Systems
{
    public class CleanUpInputDestructedSystem : ICleanupSystem
    {
        private readonly IGroup<InputEntity> _entities;

        private readonly List<InputEntity> _buffer = new(16);

        public CleanUpInputDestructedSystem(InputContext inputContext) =>
          _entities = inputContext.GetGroup(InputMatcher.Destructed);

        public void Cleanup()
        {
            var entities = _entities.GetEntities(_buffer);

            for (var i = 0; i < entities.Count; i++)
                entities[i].Destroy();
        }
    }
}