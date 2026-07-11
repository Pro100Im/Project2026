using System.Collections.Generic;
using Code.Infrastructure.View;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;

namespace Code.Infrastructure.View.Pool
{
    public class EntityViewPoolService : IEntityViewPool
    {
        private const int DefaultCapacity = 16;
        private const int MaxSize = 200;

        private readonly Transform _root;
        private readonly Dictionary<EntityBehaviour, ObjectPool<EntityBehaviour>> _pools = new(32);
        private readonly Dictionary<EntityBehaviour, EntityBehaviour> _instanceToPrefab = new(128);

        public EntityViewPoolService()
        {
            _root = new GameObject("ViewPool").transform;
        }

        public EntityBehaviour Get(EntityBehaviour prefab, Vector3 position, Quaternion rotation)
        {
            var pool = GetOrCreatePool(prefab);
            var view = pool.Get();
            view.transform.SetPositionAndRotation(position, rotation);
            return view;
        }

        public void Release(EntityBehaviour view)
        {
            if (!_instanceToPrefab.TryGetValue(view, out var prefab))
            {
                Object.Destroy(view.gameObject);
                return;
            }

            GetOrCreatePool(prefab).Release(view);
        }

        private ObjectPool<EntityBehaviour> GetOrCreatePool(EntityBehaviour prefab)
        {
            if (_pools.TryGetValue(prefab, out var pool))
                return pool;

            pool = new ObjectPool<EntityBehaviour>(
                () => CreateInstance(prefab),
                OnGet,
                OnRelease,
                OnDestroy,
                collectionCheck: false,
                DefaultCapacity,
                MaxSize);

            _pools[prefab] = pool;
            return pool;
        }

        private EntityBehaviour CreateInstance(EntityBehaviour prefab)
        {
            var view = Object.Instantiate(prefab, _root);
            _instanceToPrefab[view] = prefab;
            return view;
        }

        private static void OnGet(EntityBehaviour view) =>
            view.gameObject.SetActive(true);

        private void OnRelease(EntityBehaviour view)
        {
            view.transform.DOKill();
            ResetVisualEffects(view);
            view.gameObject.SetActive(false);
            view.transform.SetParent(_root, false);
        }

        private static void ResetVisualEffects(EntityBehaviour view)
        {
            var trails = view.GetComponentsInChildren<TrailRenderer>(true);
            for (var i = 0; i < trails.Length; i++)
                trails[i].Clear();

            var particles = view.GetComponentsInChildren<ParticleSystem>(true);
            for (var i = 0; i < particles.Length; i++)
                particles[i].Clear(true);
        }

        private void OnDestroy(EntityBehaviour view)
        {
            _instanceToPrefab.Remove(view);
            Object.Destroy(view.gameObject);
        }
    }
}
