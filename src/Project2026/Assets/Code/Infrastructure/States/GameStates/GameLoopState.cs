using Code.Game.Features;
using Code.Infrastructure.States.StateInfrastructure;
using Code.Infrastructure.Systems;

namespace Code.Infrastructure.States.GameStates
{
    public class GameLoopState : IState, ITickableState, IFixedTickableState
    {
        private readonly ISystemFactory _systems;

        private readonly GameContext _gameContext;

        private GameTickFeature _gameTickFeature;
        private GameFixedTickFeature _gameFixedTickFeature;

        public GameLoopState(ISystemFactory systems, GameContext gameContext)
        {
            _systems = systems;
            _gameContext = gameContext;
        }

        public void Enter()
        {
            _gameTickFeature = _systems.Create<GameTickFeature>();
            _gameFixedTickFeature = _systems.Create<GameFixedTickFeature>();

            _gameTickFeature.Initialize();
            _gameFixedTickFeature.Initialize();
        }

        public void Tick()
        {
            _gameTickFeature?.Execute();
            _gameTickFeature?.Cleanup();
        }

        public void FixedTick()
        {
            _gameFixedTickFeature?.Execute();
            _gameFixedTickFeature?.Cleanup();
        }

        public void Exit()
        {
            _gameTickFeature.DeactivateReactiveSystems();
            _gameTickFeature.ClearReactiveSystems();

            _gameFixedTickFeature.DeactivateReactiveSystems();
            _gameFixedTickFeature.ClearReactiveSystems();

            DestructEntities();

            _gameTickFeature.Cleanup();
            _gameTickFeature.TearDown();
            _gameTickFeature = null;

            _gameFixedTickFeature.Cleanup();
            _gameFixedTickFeature.TearDown();
            _gameFixedTickFeature = null;
        }

        private void DestructEntities()
        {
            foreach(GameEntity entity in _gameContext.GetEntities())
                entity.isDestructed = true;
        }
    }
}