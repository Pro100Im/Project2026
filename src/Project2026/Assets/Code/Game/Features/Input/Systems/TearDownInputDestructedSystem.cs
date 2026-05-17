using Entitas;
using System.Collections.Generic;

namespace Code.Game.Input.Systems
{
    public class TearDownInputDestructedSystem : ITearDownSystem
    {
        private readonly IGroup<InputEntity> _entities;

        private readonly List<InputEntity> _buffer = new(16);

        public TearDownInputDestructedSystem(InputContext inputContext) =>
          _entities = inputContext.GetGroup(InputMatcher.Input);

        public void TearDown()
        {
            var entities = _entities.GetEntities(_buffer);

            for (var i = 0; i < entities.Count; i++)
                entities[i].Destroy();
        }
    }
}