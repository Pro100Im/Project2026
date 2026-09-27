using Code.Game.Common.Entity;
using UnityEngine;

namespace Code.Infrastructure.View
{
    [RequireComponent(typeof(TownEntityBehaviour))]
    public class TownSelfInitializeEntityView : MonoBehaviour
    {
        [SerializeField] private TownEntityBehaviour _entityBehaviour;

        private void OnValidate()
        {
            if (!_entityBehaviour)
                _entityBehaviour = GetComponent<TownEntityBehaviour>();
        }

        private void Awake()
        {
            var entity = CreateTownEntity.Empty();

            _entityBehaviour.SetEntity(entity);
        }
    }
}
