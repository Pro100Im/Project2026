using Code.Infrastructure.DI.EntryPoints;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Code.Infrastructure.DI.LifetimeScopes
{
    public class BootstrupScope : LifetimeScope
    {
        [SerializeField] private string _homeScreenSceneName = "HomeScreen";

        private const string _sceneParameter = "homeScreenSceneName";

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<Bootstrup>().WithParameter(_sceneParameter, _homeScreenSceneName);
        }
    }
}