namespace Code.Infrastructure.States.StateInfrastructure
{
    public class SimpleState : IState
    {
        public virtual void Enter()
        {

        }

        protected virtual void Exit()
        {

        }

        void IExitableState.Exit()
        {
            Exit();
        }
    }
}