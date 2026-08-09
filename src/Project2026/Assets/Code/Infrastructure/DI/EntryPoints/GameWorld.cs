using Code.Game.Common.Entity;
using Code.Game.Common.UI.Transition;
using Code.Game.Features;
using Code.Game.Features.Input;
using Code.Infrastructure.Systems;
using Cysharp.Threading.Tasks;
using Entitas;
using Entitas.Unity;
using System;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using VContainer.Unity;

namespace Code.Infrastructure.DI.EntryPoints
{
    public class GameWorld : ITickable, IInitializable, IDisposable
    {
        private readonly ISystemFactory _systems;
        private readonly TransitionScreen _transitionScreen;

        private GameTickFeature _gameTickFeature;
        private readonly InputFeature _inputFeature;

        public GameWorld(TransitionScreen transitionScreen, ISystemFactory systems, InputFeature inputFeature)
        {
            _systems = systems;
            _transitionScreen = transitionScreen;
            _inputFeature = inputFeature;
        }

        public void Initialize()
        {
            CreateGameSession();
        }

        private void CreateGameSession()
        {
            var entity = CreateGameEntity.Empty();
            entity.isGameSession = true;

            _gameTickFeature = _systems.Create<GameTickFeature>();
            _gameTickFeature.ActivateReactiveSystems();
            _inputFeature.ActivateReactiveSystems();

            _gameTickFeature.Initialize();

            _transitionScreen.Hide().Forget();
        }

        public void Tick()
        {
            _gameTickFeature?.Execute();
            _gameTickFeature?.Cleanup();
        }

        public void Dispose()
        {
            _gameTickFeature.ClearReactiveSystems();
            _gameTickFeature.DeactivateReactiveSystems();
            _gameTickFeature.Cleanup();
            _gameTickFeature.TearDown();

            _inputFeature.ClearReactiveSystems();
            _inputFeature.DeactivateReactiveSystems();
            _inputFeature.Cleanup();
            _inputFeature.TearDown();

            Contexts.sharedInstance.game.Reset();
            Contexts.sharedInstance.meta.Reset();
            Contexts.sharedInstance.input.Reset();

            _gameTickFeature = null;
        }
    }
}