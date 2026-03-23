using Code.Infrastructure.States.GameStates;
using Code.Infrastructure.States.StateMachine;
using VContainer.Unity;

namespace Code.Infrastructure.DI.EntryPoints
{
    public class GameWorld : ITickable, IInitializable, IFixedTickable
    {
        private IGameStateMachine _gameStateMachine;

        public GameWorld(IGameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;
        }

        public void Initialize()
        {
            _gameStateMachine.Enter<GameEnterState>();
        }

        public void Tick()
        {
            _gameStateMachine.Tick();
        }

        public void FixedTick()
        {
            _gameStateMachine.FixedTick();
        }
    }
}