using Code.Game.Common.Entity;
using UnityEngine;

namespace Code.Infrastructure.View
{
    [RequireComponent(typeof(MetaEntityBehaviour))]
    public class MetaSelfInitializeEntityView : MonoBehaviour
    {
        [SerializeField] private MetaEntityBehaviour _entityBehaviour;

        private void OnValidate()
        {
            if (!_entityBehaviour)
                _entityBehaviour = GetComponent<MetaEntityBehaviour>();
        }

        private void Awake()
        {
            var entity = CreateMetaEntity.Empty();

            _entityBehaviour.SetEntity(entity);
        }
    }
}
