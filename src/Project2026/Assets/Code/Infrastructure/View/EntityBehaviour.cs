using Code.Infrastructure.View.Registrars;
using UnityEngine;

namespace Code.Infrastructure.View
{
    public class EntityBehaviour : MonoBehaviour, IEntityView
    {
        private GameEntity _entity;

        public GameEntity Entity => _entity;
        public GameObject GameObject => gameObject;

        public void SetEntity(GameEntity entity)
        {
            _entity = entity;
            _entity.AddView(this);
            _entity.Retain(this);

            foreach(IEntityComponentRegistrar registrar in GetComponentsInChildren<IEntityComponentRegistrar>())
                registrar.RegisterComponents();
        }

        public void ReleaseEntity()
        {
            foreach(IEntityComponentRegistrar registrar in GetComponentsInChildren<IEntityComponentRegistrar>())
                registrar.UnregisterComponents();

            _entity.Release(this);
            _entity = null;
        }
    }
}