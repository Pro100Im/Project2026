using Code.Infrastructure.Systems;
using Code.Meta.UI.MainMenu;
using VContainer;
using VContainer.Unity;

namespace Code.Infrastructure.DI.LifetimeScopes
{
    public class MenuScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<ISystemFactory, SystemFactory>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<LevelSelectMenu>();

            builder.RegisterEntryPoint<MenuWorld>();
        }
    }
}
