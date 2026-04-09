using Code.Common.Entity;
using Code.Infrastructure.View.Registrars;
using UnityEngine;

namespace Code.Game.Common.Registrars
{
    public class SelfDestructRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private float _selfDestructDuration;

        public override void RegisterComponents()
        {
            var entity = CreateGameEntity.Empty();

            entity.AddTargetId(Entity.id.Value);
            entity.AddDuration(_selfDestructDuration);
            entity.isDelayDestruct = true;
        }

        public override void UnregisterComponents()
        {
            
        }
    }
}