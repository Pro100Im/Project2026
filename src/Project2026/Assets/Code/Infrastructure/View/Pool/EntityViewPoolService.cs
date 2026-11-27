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

        private static readonly List<TrailRenderer> TrailsBuffer = new(16);
        private static readonly List<ParticleSystem> ParticlesBuffer = new(32);

        private Transform _root;
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

        public void Clear()
        {
            // Scene unload may already destroy pooled GOs; skip pool.Clear() to avoid
            // MissingReferenceException in actionOnDestroy. Destroying the root is enough.
            _pools.Clear();
            _instanceToPrefab.Clear();

            if (_root != null)
            {
                if (_root)
                    Object.Destroy(_root.gameObject);
                _root = null;
            }
        }

        private ObjectPool<EntityBehaviour> GetOrCreatePool(EntityBehaviour prefab)
        {
            if (_pools.TryGetValue(prefab, out var pool))
                return pool;

            pool = new ObjectPool<EntityBehaviour>(
                createFunc: () => CreateInstance(prefab),
                actionOnGet: null,
                actionOnRelease: OnRelease,
                actionOnDestroy: OnDestroy,
                collectionCheck: false,
                defaultCapacity: DefaultCapacity,
                maxSize: MaxSize);

            _pools[prefab] = pool;
            return pool;
        }

        private EntityBehaviour CreateInstance(EntityBehaviour prefab)
        {
            var view = Object.Instantiate(prefab, _root);
            view.gameObject.SetActive(false);
            _instanceToPrefab[view] = prefab;
            return view;
        }

        private void OnRelease(EntityBehaviour view)
        {
            view.transform.DOKill();
            ResetVisualEffects(view);
            view.gameObject.SetActive(false);
            view.transform.SetParent(_root, false);
        }

        private static void ResetVisualEffects(EntityBehaviour view)
        {
            // List overloads fill reused buffers instead of allocating an array on every release.
            view.GetComponentsInChildren(true, TrailsBuffer);
            for (var i = 0; i < TrailsBuffer.Count; i++)
                TrailsBuffer[i].Clear();

            view.GetComponentsInChildren(true, ParticlesBuffer);
            for (var i = 0; i < ParticlesBuffer.Count; i++)
                ParticlesBuffer[i].Clear(true);

            TrailsBuffer.Clear();
            ParticlesBuffer.Clear();
        }

        private void OnDestroy(EntityBehaviour view)
        {
            _instanceToPrefab.Remove(view);
            if (view != null)
                Object.Destroy(view.gameObject);
        }
    }
}
