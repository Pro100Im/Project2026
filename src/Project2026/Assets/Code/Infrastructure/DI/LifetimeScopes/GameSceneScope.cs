using Code.Common.Cameras;
using Code.Common.UI.Transition;
using Code.Game.Features;
using Code.Game.Features.Enemy.Factory;
using Code.Game.Features.Enemy.Systems;
using Code.Game.Features.Input;
using Code.Game.Features.Input.Systems;
using Code.Game.Features.Movement;
using Code.Game.Features.Player;
using Code.Game.Features.Player.Systems;
using Code.Game.Features.Spawn;
using Code.Game.Features.Spawn.Systems;
using Code.Game.Features.Wave;
using Code.Game.Features.Wave.Systems;
using Code.Game.Input.Service;
using Code.Game.StaticData.Configs;
using Code.Infrastructure.DI.EntryPoints;
using Code.Infrastructure.Identifiers;
using Code.Infrastructure.States.Factory;
using Code.Infrastructure.States.GameStates;
using Code.Infrastructure.States.StateMachine;
using Code.Infrastructure.Systems;
using Code.Infrastructure.View;
using Code.Infrastructure.View.Factory;
using Code.Infrastructure.View.Systems;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Code.Infrastructure.DI.LifetimeScopes
{
    public class GameSceneScope : LifetimeScope
    {
        [SerializeField] private WavesConfig _wavesConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            BindServices(builder);

            BindGameConfigs(builder);
            BindContexts(builder);

            BindGameStates(builder);
            BindStateMachine(builder);

            BindFeatures(builder);

            BindSystems(builder);

            BindGameFactories(builder);

            builder.RegisterEntryPoint<GameWorld>();
        }

        private void BindContexts(IContainerBuilder builder)
        {
            builder.RegisterInstance(Contexts.sharedInstance);
            builder.RegisterInstance(Contexts.sharedInstance.game);
            builder.RegisterInstance(Contexts.sharedInstance.input);
            builder.RegisterInstance(Contexts.sharedInstance.meta);
        }

        private void BindServices(IContainerBuilder builder)
        {
            builder.Register<IIdentifierService, IdentifierService>(Lifetime.Singleton);
            builder.Register<IInputService, InputService>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<ICameraService>().AsImplementedInterfaces().AsSelf();
            builder.RegisterComponentInHierarchy<TransitionScreen>();
        }

        private void BindStateMachine(IContainerBuilder builder)
        {
            builder.Register<GameStateMachine>(Lifetime.Singleton).AsImplementedInterfaces();
        }

        private void BindGameStates(IContainerBuilder builder)
        {
            builder.Register<GameEnterState>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<GameLoopState>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<GameOverState>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        }

        private void BindFeatures(IContainerBuilder builder)
        {
            builder.Register<GameTickFeature>(Lifetime.Singleton);
            builder.Register<GameFixedTickFeature>(Lifetime.Singleton);

            builder.Register<InputFeature>(Lifetime.Singleton);
            builder.Register<PlayerFeature>(Lifetime.Singleton);

            builder.Register<WaveFeature>(Lifetime.Singleton);
            builder.Register<SpawnFeature>(Lifetime.Singleton);
            builder.Register<CreateViewFeature>(Lifetime.Singleton);
            builder.Register<MovementFeature>(Lifetime.Singleton);
        }

        private void BindSystems(IContainerBuilder builder)
        {
            builder.Register<CreateEntityViewFromPathSystem>(Lifetime.Singleton);
            builder.Register<CreateEntityViewFromPrefabSystem>(Lifetime.Singleton);

            builder.Register<InitializeInputSystem>(Lifetime.Singleton);
            builder.Register<EmitInputSystem>(Lifetime.Singleton);

            builder.Register<PlayerCameraInitSystem>(Lifetime.Singleton);

            builder.Register<WaveSystem>(Lifetime.Singleton);

            builder.Register<EnemyAnimatorSystem>(Lifetime.Singleton);
            builder.Register<EnemySelectSpawnPosSystem>(Lifetime.Singleton);
            builder.Register<EnemySpawnSystem>(Lifetime.Singleton);
        }

        private void BindGameFactories(IContainerBuilder builder)
        {
            builder.Register<IStateFactory, StateFactory>(Lifetime.Singleton);
            builder.Register<ISystemFactory, SystemFactory>(Lifetime.Singleton);

            builder.Register<IEntityViewFactory, EntityViewFactory>(Lifetime.Singleton);
            builder.Register<EnemyFactory>(Lifetime.Singleton);
        }

        private void BindGameConfigs(IContainerBuilder builder)
        {
            builder.RegisterInstance(_wavesConfig).AsSelf();
        }
    }
}