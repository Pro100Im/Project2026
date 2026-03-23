using UnityEngine;

namespace Code.Infrastructure.View
{
    [RequireComponent(typeof(EntityBehaviour))]
    public abstract class EntityDependant : MonoBehaviour
    {
        public EntityBehaviour EntityView;

        public GameEntity Entity => EntityView != null ? EntityView.Entity : null;

        private void OnValidate()
        {
            if (!EntityView)
                EntityView = GetComponent<EntityBehaviour>();
        }
    }
}