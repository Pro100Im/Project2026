using UnityEngine;

namespace Code.Infrastructure.View
{
    [RequireComponent(typeof(TownEntityBehaviour))]
    public abstract class TownEntityDependant : MonoBehaviour
    {
        public TownEntityBehaviour EntityView;

        public TownEntity Entity => EntityView != null ? EntityView.Entity : null;

        private void OnValidate()
        {
            if (!EntityView)
                EntityView = GetComponent<TownEntityBehaviour>();
        }
    }
}
