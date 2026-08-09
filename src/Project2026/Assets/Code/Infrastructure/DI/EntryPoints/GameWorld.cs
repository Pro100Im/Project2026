using Code.Game.Common.Entity;
using Code.Game.Common.UI.Transition;
using Code.Game.Features;
using Code.Game.Features.Input;
using Code.Infrastructure.Systems;
using Cysharp.Threading.Tasks;
using System;
using VContainer.Unity;

namespace Code.Infrastructure.DI.EntryPoints
{
    public class GameWorld : ITickable, IInitializable, IDisposable
    {
        private readonly ISystemFactory _systems;
        private readonly TransitionScreen _transitionScreen;
        private readonly GameContext _gameContext;

        private GameTickFeature _gameTickFeature;
        private readonly InputFeature _inputFeature;

        public GameWorld(TransitionScreen transitionScreen, ISystemFactory systems, InputFeature inputFeature)
        {
            _systems = systems;
            _gameContext = Contexts.sharedInstance.game;
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
            _gameTickFeature.DeactivateReactiveSystems();
            _gameTickFeature.ClearReactiveSystems();
            _gameTickFeature.Cleanup();
            _gameTickFeature.TearDown();

            _inputFeature.Cleanup();
            _inputFeature.TearDown();

            foreach (GameEntity entity in Contexts.sharedInstance.game.GetEntities())
                entity.Destroy();

            foreach (MetaEntity entity in Contexts.sharedInstance.meta.GetEntities())
                entity.Destroy();

            foreach (InputEntity entity in Contexts.sharedInstance.input.GetEntities())
                entity.Destroy();

            _gameTickFeature = null;
        }
    }
}