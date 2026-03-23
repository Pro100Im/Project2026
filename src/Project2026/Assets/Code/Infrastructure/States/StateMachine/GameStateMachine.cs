using Code.Infrastructure.States.Factory;
using Code.Infrastructure.States.StateInfrastructure;

namespace Code.Infrastructure.States.StateMachine
{
    public class GameStateMachine : IGameStateMachine
    {
        private readonly IStateFactory _stateFactory;
        private IExitableState _activeState;

        public GameStateMachine(IStateFactory stateFactory)
        {
            _stateFactory = stateFactory;
        }

        public void Enter<TState>() where TState : class, IState
        {
            var state = ChangeState<TState>();

            state.Enter();
        }

        public void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadState<TPayload>
        {
            var state = ChangeState<IPayloadState<TPayload>>();

            state.Enter(payload);
        }

        private TState ChangeState<TState>() where TState : class, IExitableState
        {
            _activeState?.Exit();

            var state = _stateFactory.GetState<TState>();

            _activeState = state;

            return state;
        }

        public void Tick()
        {
            if (_activeState is ITickableState state)
                state.Tick();
        }

        public void FixedTick()
        {
            if (_activeState is IFixedTickableState state)
                state.FixedTick();
        }
    }
}