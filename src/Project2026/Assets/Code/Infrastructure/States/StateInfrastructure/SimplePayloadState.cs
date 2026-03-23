namespace Code.Infrastructure.States.StateInfrastructure
{
    public class SimplePayloadState<TPayload>
    {
        public virtual void Enter(TPayload payload)
        {

        }

        protected virtual void Exit()
        {

        }
    }
}