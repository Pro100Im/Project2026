using Code.Infrastructure.View;
using UnityEngine;

namespace Code.Infrastructure.View.Pool
{
    public interface IEntityViewPool
    {
        EntityBehaviour Get(EntityBehaviour prefab, Vector3 position, Quaternion rotation);
        void Release(EntityBehaviour view);
    }
}
