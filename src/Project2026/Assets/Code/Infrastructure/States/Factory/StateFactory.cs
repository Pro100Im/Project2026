using Code.Infrastructure.States.StateInfrastructure;
using VContainer;

namespace Code.Infrastructure.States.Factory
{
    public class StateFactory : IStateFactory
    {
        private readonly IObjectResolver _objectResolver;

        public StateFactory(IObjectResolver objectResolver)
        {
            _objectResolver = objectResolver;
        }

        public T GetState<T>() where T : class, IExitableState
        {
            return _objectResolver.Resolve<T>();
        }
    }
}