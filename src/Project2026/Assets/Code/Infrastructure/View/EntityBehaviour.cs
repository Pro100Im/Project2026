using UnityEngine;

namespace Code.Infrastructure.View
{
    public class EntityBehaviour : EntityBehaviourBase<GameEntity>, IEntityView
    {
        public GameObject GameObject => gameObject;

        protected override void OnEntityBound(GameEntity entity) =>
            entity.AddView(this);
    }
}
