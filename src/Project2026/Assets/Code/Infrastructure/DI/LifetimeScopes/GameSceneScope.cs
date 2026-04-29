using Code.Game.Common.Cameras;
using Code.Game.Common.Destruct;
using Code.Game.Common.Destruct.Systems;
using Code.Game.Common.UI.Transition;
using Code.Game.Features;
using Code.Game.Features.Animator;
using Code.Game.Features.Animator.Systems;
using Code.Game.Features.Attack;
using Code.Game.Features.Attack.Systems;
using Code.Game.Features.Cooldown;
using Code.Game.Features.Cooldown.Systems;
using Code.Game.Features.Damage;
using Code.Game.Features.Damage.Systems;
using Code.Game.Features.Death;
using Code.Game.Features.Death.Systems;
using Code.Game.Features.Duration;
using Code.Game.Features.Duration.Systems;
using Code.Game.Features.Health;
using Code.Game.Features.Health.Systems;
using Code.Game.Features.Input;
using Code.Game.Features.Input.Systems;
using Code.Game.Features.Movement;
using Code.Game.Features.Movement.Systems;
using Code.Game.Features.Player;
using Code.Game.Features.Player.Systems;
using Code.Game.Features.Spawn;
using Code.Game.Features.Spawn.Systems;
using Code.Game.Features.Target;
using Code.Game.Features.Target.Services;
using Code.Game.Features.Target.Systems;
using Code.Game.Features.Tower;
using Code.Game.Features.Tower.Systems;
using Code.Game.Features.Wave;
using Code.Game.Features.Wave.Systems;
using Code.Game.Input.Service;
using Code.Game.Input.Systems;
using Code.Game.StaticData.Configs;
using Code.Infrastructure.DI.EntryPoints;
using Code.Infrastructure.States.Factory;
using Code.Infrastructure.States.GameStates;
using Code.Infrastructure.States.StateMachine;
using Code.Infrastructure.Systems;
using Code.Infrastructure.View;
using Code.Infrastructure.View.Systems;
using Code.Meta.Features.Game;
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
            builder.Register<IInputService, InputService>(Lifetime.Singleton);
            builder.Register<TargetService>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<ICameraService>().AsImplementedInterfaces().AsSelf();
            builder.RegisterComponentInHierarchy<TransitionScreen>();
            builder.RegisterComponentInHierarchy<GameScreen>();
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

            builder.Register<TowerFeature>(Lifetime.Singleton);

            builder.Register<CooldownFeature>(Lifetime.Singleton);
            builder.Register<DurationFeature>(Lifetime.Singleton);

            builder.Register<WaveFeature>(Lifetime.Singleton);
            builder.Register<SpawnFeature>(Lifetime.Singleton);
            builder.Register<CreateViewFeature>(Lifetime.Singleton);

            builder.Register<AnimatorFeature>(Lifetime.Singleton);

            builder.Register<MovementFeature>(Lifetime.Singleton);

            builder.Register<TargetFeature>(Lifetime.Singleton);
            builder.Register<AttackFeature>(Lifetime.Singleton);
            builder.Register<DamageFeature>(Lifetime.Singleton);

            builder.Register<HealthFeature>(Lifetime.Singleton);

            builder.Register<DeathFeature>(Lifetime.Singleton);

            builder.Register<ProcessDestructedFeature>(Lifetime.Singleton);
        }

        private void BindSystems(IContainerBuilder builder)
        {
            builder.Register<CreateEntityViewFromPathSystem>(Lifetime.Singleton);
            builder.Register<CreateEntityViewFromPrefabSystem>(Lifetime.Singleton);

            builder.Register<InitializeInputSystem>(Lifetime.Singleton);
            builder.Register<InputClickOnEntitySystem>(Lifetime.Singleton);
            builder.Register<TowerMenuSystem>(Lifetime.Singleton);
            builder.Register<CleanUpInputDestructedSystem>(Lifetime.Singleton);
            builder.Register<TearDownInputDestructedSystem>(Lifetime.Singleton);

            builder.Register<PlayerCameraInitSystem>(Lifetime.Singleton);

            builder.Register<WaveInitSystem>(Lifetime.Singleton);
            builder.Register<WaveStartSystem>(Lifetime.Singleton);
            builder.Register<WaveProgressSystem>(Lifetime.Singleton);

            builder.Register<TowerBuildSystem>(Lifetime.Singleton);

            builder.Register<EnemySelectSpawnPosSystem>(Lifetime.Singleton);
            builder.Register<EnemySpawnSystem>(Lifetime.Singleton);

            builder.Register<CharacterAnimatorSystem>(Lifetime.Singleton);
            builder.Register<PlayerCastleAnimatorSystem>(Lifetime.Singleton);

            builder.Register<SelectTargetCellSystem>(Lifetime.Singleton);
            builder.Register<OccupiedCellSystem>(Lifetime.Singleton);
            builder.Register<ReservedCellSystem>(Lifetime.Singleton);
            builder.Register<MovementSystem>(Lifetime.Singleton);
            builder.Register<AttachPosToTargetSystem>(Lifetime.Singleton);

            builder.Register<BuildFlowFieldSystem>(Lifetime.Singleton);
            builder.Register<SearchingClosestTargetSystem>(Lifetime.Singleton);
            builder.Register<UpdateTargetSystem>(Lifetime.Singleton);

            builder.Register<AttackStartSystem>(Lifetime.Singleton);
            builder.Register<AttackProcessSystem>(Lifetime.Singleton);
            builder.Register<AttackEndSystem>(Lifetime.Singleton);

            builder.Register<ApplyDamageSystem>(Lifetime.Singleton);

            builder.Register<HealthBarSystem>(Lifetime.Singleton);

            builder.Register<DeathSystem>(Lifetime.Singleton);

            builder.Register<CooldownLeftSystem>(Lifetime.Singleton);
            builder.Register<DurationLeftSystem>(Lifetime.Singleton);

            builder.Register<DelayDestructSystem>(Lifetime.Singleton);
            builder.Register<MetaDestructedSystem>(Lifetime.Singleton);
            builder.Register<GameDestructedViewSystem>(Lifetime.Singleton);
            builder.Register<GameDestructedSystem>(Lifetime.Singleton);
        }

        private void BindGameFactories(IContainerBuilder builder)
        {
            builder.Register<IStateFactory, StateFactory>(Lifetime.Singleton);
            builder.Register<ISystemFactory, SystemFactory>(Lifetime.Singleton);
        }

        private void BindGameConfigs(IContainerBuilder builder)
        {
            builder.RegisterInstance(_wavesConfig).AsSelf();
        }
    }
}