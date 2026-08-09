using System.Collections.Generic;
using Entitas;

namespace Code.Game.Common.Destruct.Systems
{
    public class MetaDestructedSystem : ICleanupSystem
    {
        private readonly IGroup<MetaEntity> _entities;

        private readonly List<MetaEntity> _entitiesBuffer = new(16);

        public MetaDestructedSystem() =>
          _entities = Contexts.sharedInstance.meta.GetGroup(MetaMatcher.Destructed);

        public void Cleanup()
        {
            var entities = _entities.GetEntities(_entitiesBuffer);

            for (var i = 0; i < entities.Count; i++)
                entities[i].Destroy();
        }
    }
}