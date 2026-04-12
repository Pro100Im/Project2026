using Code.Game.Common.UI.Transition;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Code.Infrastructure.DI.LifetimeScopes
{
    public class MenuScope : LifetimeScope
    {
        [SerializeField] private string _gameScreenSceneName = "Game";

        private const string _sceneParameter = "gameScreenSceneName";

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<TransitionScreen>();
            builder.RegisterEntryPoint<MenuWorld>().WithParameter(_sceneParameter, _gameScreenSceneName);
        }
    }
}
