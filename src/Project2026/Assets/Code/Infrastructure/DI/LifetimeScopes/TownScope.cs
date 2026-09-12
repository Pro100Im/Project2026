using Code.Infrastructure.DI.EntryPoints;
using Code.Infrastructure.Systems;
using Code.Infrastructure.View.Pool;
using VContainer;
using VContainer.Unity;

namespace Code.Infrastructure.DI.LifetimeScopes
{
    public class TownScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            BindServices(builder);
            BindGameFactories(builder);

            builder.RegisterEntryPoint<TownWorld>();
        }

        private void BindServices(IContainerBuilder builder)
        {
            builder.Register<IEntityViewPool, EntityViewPoolService>(Lifetime.Singleton);
        }

        private void BindGameFactories(IContainerBuilder builder)
        {
            builder.Register<ISystemFactory, SystemFactory>(Lifetime.Singleton);
        }
    }
}
