using Code.Infrastructure.View.Registrars;
using Entitas;
using UnityEngine;

namespace Code.Infrastructure.View
{
    public abstract class EntityBehaviourBase<TEntity> : MonoBehaviour where TEntity : Entity
    {
        private TEntity _entity;

        public TEntity Entity => _entity;

        public void SetEntity(TEntity entity)
        {
            _entity = entity;
            OnEntityBound(entity);
            _entity.Retain(this);

            foreach (IEntityComponentRegistrar registrar in GetComponentsInChildren<IEntityComponentRegistrar>())
                registrar.RegisterComponents();
        }

        public void ReleaseEntity()
        {
            foreach (IEntityComponentRegistrar registrar in GetComponentsInChildren<IEntityComponentRegistrar>())
                registrar.UnregisterComponents();

            OnEntityReleased(_entity);
            _entity.Release(this);
            _entity = null;
        }

        private void OnDestroy()
        {
            if (_entity != null)
                ReleaseEntity();
        }

        protected virtual void OnEntityBound(TEntity entity) { }

        protected virtual void OnEntityReleased(TEntity entity) { }
    }
}
