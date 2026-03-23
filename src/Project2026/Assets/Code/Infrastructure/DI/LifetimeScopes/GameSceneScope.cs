using Code.Common.Cameras;
using Code.Common.UI.Transition;
using Code.Game.Features;
using Code.Game.Features.Input;
using Code.Game.Features.Input.Systems;
using Code.Game.Features.Movement;
using Code.Game.Features.Network;
using Code.Game.Features.Network.Systems;
using Code.Game.Features.Player;
using Code.Game.Features.Player.Factory;
using Code.Game.Features.Player.Systems;
using Code.Game.Features.Spawn;
using Code.Game.Input.Service;
using Code.Infrastructure.DI.EntryPoints;
using Code.Infrastructure.Identifiers;
using Code.Infrastructure.States.Factory;
using Code.Infrastructure.States.GameStates;
using Code.Infrastructure.States.StateMachine;
using Code.Infrastructure.Systems;
using Code.Infrastructure.View;
using Code.Infrastructure.View.Factory;
using Code.Infrastructure.View.Systems;
using VContainer;
using VContainer.Unity;

namespace Code.Infrastructure.DI.LifetimeScopes
{
    public class GameSceneScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IIdentifierService, IdentifierService>(Lifetime.Singleton);

            BindContexts(builder);

            BindSystemFactory(builder);
            BindStateFactory(builder);

            BindServices(builder);
            BindGameStates(builder);
            BindStateMachine(builder);

            BindFeatures(builder);

            BindServerSystems(builder);
            BindClientSystems(builder);
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
            builder.RegisterInstance(Contexts.sharedInstance.network);
        }

        private void BindStateFactory(IContainerBuilder builder)
        {
            builder.Register<IStateFactory, StateFactory>(Lifetime.Singleton);
        }

        private void BindSystemFactory(IContainerBuilder builder)
        {
            builder.Register<ISystemFactory, SystemFactory>(Lifetime.Singleton);
        }

        private void BindServices(IContainerBuilder builder)
        {
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

            builder.Register<CreateViewFeature>(Lifetime.Singleton);
            builder.Register<NetworkFeature>(Lifetime.Singleton);
            builder.Register<NetworkFixedFeature>(Lifetime.Singleton);
            builder.Register<SpawnFeature>(Lifetime.Singleton);
            builder.Register<InputFeature>(Lifetime.Singleton);
            builder.Register<PlayerFeature>(Lifetime.Singleton);
            builder.Register<MovementFeature>(Lifetime.Singleton);
        }

        private void BindServerSystems(IContainerBuilder builder)
        {
            builder.Register<CreateEntityViewFromPathSystem>(Lifetime.Singleton);
            builder.Register<CreateEntityViewFromPrefabSystem>(Lifetime.Singleton);

            builder.Register<SelectPlayerSpawnPositionSystem>(Lifetime.Singleton);
        }

        private void BindClientSystems(IContainerBuilder builder)
        {
            builder.Register<InitializeInputSystem>(Lifetime.Singleton);
            builder.Register<EmitInputSystem>(Lifetime.Singleton);

            builder.Register<PlayerCameraInitSystem>(Lifetime.Singleton);
            builder.Register<PlayerAnimatorSystem>(Lifetime.Singleton);
        }

        private void BindSystems(IContainerBuilder builder)
        {
            builder.Register<ObjectIdReceiveSystem>(Lifetime.Singleton);

            builder.Register<PlayerCharacterLinkSystem>(Lifetime.Singleton);
            builder.Register<SetPlayerSpawnedPositionSystem>(Lifetime.Singleton);
        }

        private void BindGameFactories(IContainerBuilder builder)
        {
            builder.Register<IEntityViewFactory, EntityViewFactory>(Lifetime.Singleton);
            builder.Register<IPlayerFactory, PlayerFactory>(Lifetime.Singleton);
        }
    }
}