using Code.Game.Common.Entity;
using Code.Game.Common.UI.Transition;
using Code.Infrastructure.States.StateInfrastructure;
using Code.Infrastructure.States.StateMachine;
using Cysharp.Threading.Tasks;

namespace Code.Infrastructure.States.GameStates
{
    public class GameEnterState : SimpleState
    {
        private readonly IGameStateMachine _stateMachine;

        private readonly TransitionScreen _transitionScreen;

        public GameEnterState(IGameStateMachine stateMachine, TransitionScreen transitionScreen,
            GameContext game)
        {
            _stateMachine = stateMachine;
            _transitionScreen = transitionScreen;
        }

        public override void Enter()
        {
            CreateGameSession();

            _stateMachine.Enter<GameLoopState>();
        }

        private void CreateGameSession()
        {
            var entity = CreateGameEntity.Empty();

            entity.isGameSession = true;
        }

        protected override void Exit()
        {
            _transitionScreen.Hide().AsTask();
        }
    }
}
