using Code.Common.StaticData;
using Code.Common.Time;
using Code.Common.UI;
using Code.Infrastructure.AssetManagement;
using Code.Infrastructure.Helpers;
using Code.Infrastructure.Loading;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Code.Infrastructure.DI.LifetimeScopes
{
    public class GlobalScope : LifetimeScope
    {
        [SerializeField] private CoroutineRunner _coroutineRunner;

        protected override void Configure(IContainerBuilder builder)
        {
            BindUIFactories(builder);

            BindCommonServices(builder);
            BindAssetManagementServices(builder);

            builder.RegisterComponentInNewPrefab(_coroutineRunner, Lifetime.Singleton).DontDestroyOnLoad().AsImplementedInterfaces();
        }

        private void BindUIFactories(IContainerBuilder builder)
        {

        }

        private void BindCommonServices(IContainerBuilder builder)
        {
            builder.Register<ITimeService, UnityTimeService>(Lifetime.Singleton);
            builder.Register<ISceneLoader, SceneLoader>(Lifetime.Singleton);
            builder.Register<IStaticDataService, StaticDataService>(Lifetime.Singleton);

            builder.Register<UIService>(Lifetime.Singleton);
        }

        private void BindAssetManagementServices(IContainerBuilder builder)
        {
            builder.Register<IAssetProvider, AssetProvider>(Lifetime.Singleton);
        }
    }
}
