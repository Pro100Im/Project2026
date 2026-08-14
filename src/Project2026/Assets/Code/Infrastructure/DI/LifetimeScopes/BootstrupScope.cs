using Code.Infrastructure.DI.EntryPoints;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Code.Infrastructure.DI.LifetimeScopes
{
    public class BootstrupScope : LifetimeScope
    {
        [SerializeField] private string _homeScreenSceneName = "HomeScreen";
        [SerializeField] private string _townSceneName = "Town";

        private const string _homeSceneParameter = "homeScreenSceneName";
        private const string _townSceneParameter = "townSceneName";

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<Bootstrup>()
                .WithParameter(_homeSceneParameter, _homeScreenSceneName)
                .WithParameter(_townSceneParameter, _townSceneName);
        }
    }
}