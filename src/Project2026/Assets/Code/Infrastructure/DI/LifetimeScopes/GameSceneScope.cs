using Code.Game.StaticData.Configs;
using Code.Game.Features.Target.Services;
using Code.Infrastructure.DI.EntryPoints;
using Code.Infrastructure.Factories;
using Code.Infrastructure.Systems;
using Code.Infrastructure.View.Pool;
using Code.Meta.Features.Game;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Code.Infrastructure.DI.LifetimeScopes
{
    public class GameSceneScope : LifetimeScope
    {
        [SerializeField] private FloatingTextConfig _floatingTextConfig;
        [SerializeField] private BattleOverlayConfig _battleOverlayConfig;
        [SerializeField] private Transform _levelRoot;

        protected override void Configure(IContainerBuilder builder)
        {
            BindServices(builder);
            BindGameConfigs(builder);
            BindGameFactories(builder);

            builder.RegisterEntryPoint<GameWorld>();
        }

        private void BindServices(IContainerBuilder builder)
        {
            builder.Register<IEntityViewPool, EntityViewPoolService>(Lifetime.Singleton);
            builder.Register<TargetService>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<GameScreen>();
            builder.RegisterComponentInHierarchy<UnitRangeView>();
        }

        private void BindGameFactories(IContainerBuilder builder)
        {
            builder.Register<ISystemFactory, SystemFactory>(Lifetime.Singleton);
            builder.RegisterInstance(_levelRoot);
            builder.Register<ILevelFactory, LevelFactory>(Lifetime.Singleton);
        }

        private void BindGameConfigs(IContainerBuilder builder)
        {
            builder.RegisterInstance(_floatingTextConfig).AsSelf();
            builder.RegisterInstance(_battleOverlayConfig).AsSelf();
        }
    }
}
