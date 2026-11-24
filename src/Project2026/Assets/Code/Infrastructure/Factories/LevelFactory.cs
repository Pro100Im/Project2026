using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Code.Infrastructure.Factories
{
    public class LevelFactory : ILevelFactory
    {
        private readonly IObjectResolver _resolver;
        private readonly Transform _levelRoot;

        public LevelFactory(IObjectResolver resolver, Transform levelRoot)
        {
            _resolver = resolver;
            _levelRoot = levelRoot;
        }

        public GameObject Create(GameObject prefab)
        {
            if (prefab == null)
                return null;

            var instance = _resolver.Instantiate(prefab, _levelRoot);
            instance.name = prefab.name;
            return instance;
        }
    }
}
