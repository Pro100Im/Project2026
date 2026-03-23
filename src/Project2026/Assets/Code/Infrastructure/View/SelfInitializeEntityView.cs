using Code.Common.Entity;
using Code.Infrastructure.Identifiers;
using UnityEngine;
using VContainer;

namespace Code.Infrastructure.View
{
    [RequireComponent(typeof(EntityBehaviour))]
    public class SelfInitializeEntityView : MonoBehaviour
    {
        [SerializeField] private EntityBehaviour _entityBehaviour;

        private IIdentifierService _identifierService;

        [Inject]
        private void Construct(IIdentifierService identifierService)
        {
            _identifierService = identifierService;
        }

        private void OnValidate()
        {
            if (!_entityBehaviour)
                _entityBehaviour = GetComponent<EntityBehaviour>();
        }

        private void Awake()
        {
            var entity = CreateGameEntity.Empty();

            entity.AddId(_identifierService.Next());

            _entityBehaviour.SetEntity(entity);
        }
    }
}