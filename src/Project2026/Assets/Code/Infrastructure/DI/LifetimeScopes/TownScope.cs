using Code.Game.Features;
using Code.Game.Features.Target.Services;
using Code.Game.Features.Town;
using Code.Game.Features.Town.Systems;
using Code.Game.Input.Service;
using Code.Infrastructure.DI.EntryPoints;
using Code.Infrastructure.Systems;
using Code.Infrastructure.View.Pool;
using Code.Infrastructure.View.Systems;
using VContainer;
using VContainer.Unity;

namespace Code.Infrastructure.DI.LifetimeScopes
{
    public class TownScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            BindServices(builder);

            BindFeatures(builder);

            BindSystems(builder);

            BindGameFactories(builder);

            builder.RegisterEntryPoint<TownWorld>();
        }

        private void BindServices(IContainerBuilder builder)
        {
            builder.Register<IInputService, InputService>(Lifetime.Singleton);
            builder.Register<IEntityViewPool, EntityViewPoolService>(Lifetime.Singleton);
            builder.Register<TargetService>(Lifetime.Singleton);
        }

        private void BindFeatures(IContainerBuilder builder)
        {
            builder.Register<TownFeature>(Lifetime.Singleton);
        }

        private void BindSystems(IContainerBuilder builder)
        {
            builder.Register<TownTestSystem>(Lifetime.Singleton); 
        }

        private void BindGameFactories(IContainerBuilder builder)
        {
            builder.Register<ISystemFactory, SystemFactory>(Lifetime.Singleton);
        }
    }
}