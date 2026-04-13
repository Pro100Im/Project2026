using Code.Game.Common.Entity;
using Code.Infrastructure.Identifiers;
using UnityEngine;

namespace Code.Infrastructure.View
{
    [RequireComponent(typeof(EntityBehaviour))]
    public class SelfInitializeEntityView : MonoBehaviour
    {
        [SerializeField] private EntityBehaviour _entityBehaviour;

        private void OnValidate()
        {
            if (!_entityBehaviour)
                _entityBehaviour = GetComponent<EntityBehaviour>();
        }

        private void Awake()
        {
            var entity = CreateGameEntity.Empty();

            entity.AddId(EntityIdentifier.Next());

            _entityBehaviour.SetEntity(entity);
        }
    }
}