using UnityEngine;

namespace Code.Infrastructure.View
{
    [RequireComponent(typeof(MetaEntityBehaviour))]
    public abstract class MetaEntityDependant : MonoBehaviour
    {
        public MetaEntityBehaviour EntityView;

        public MetaEntity Entity => EntityView != null ? EntityView.Entity : null;

        private void OnValidate()
        {
            if (!EntityView)
                EntityView = GetComponent<MetaEntityBehaviour>();
        }
    }
}
