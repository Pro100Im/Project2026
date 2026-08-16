using Code.Game.Common.UI.Transition;
using VContainer;
using VContainer.Unity;

namespace Code.Infrastructure.DI.LifetimeScopes
{
    public class TownScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            //builder.RegisterEntryPoint<MenuWorld>().WithParameter(_sceneParameter, _gameScreenSceneName);
        }
    }
}