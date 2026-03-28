using Code.Common.Entity;
using Code.Common.UI.Transition;
using Code.Infrastructure.States.StateInfrastructure;
using Code.Infrastructure.States.StateMachine;
using Cysharp.Threading.Tasks;
using UnityEngine;

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
            CreatePlayer();

            _stateMachine.Enter<GameLoopState>();
        }

        private void CreatePlayer()
        {
            var entity = CreateInputEntity.Empty();

            entity.isInput = true;
            entity.AddAxisInput(Vector2.zero);
            entity.AddPointerInput(Vector2.zero);
            entity.AddPointerRay(new Ray());
        }

        protected override void Exit()
        {
            _transitionScreen.Hide().AsTask();
        }
    }
}