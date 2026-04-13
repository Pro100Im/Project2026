using Code.Game.StaticData.Configs;
using Code.Infrastructure.View.Registrars;
using UnityEngine;

namespace Code.Game.Common.Entity
{
    public class EntityConfigRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private EntityConfig _config;

        public override void RegisterComponents()
        {
            foreach (var property in _config.Properties)
                property.Apply(Entity);
        }

        public override void UnregisterComponents()
        {
            Entity.RemoveAllComponents();
        }
    }
}