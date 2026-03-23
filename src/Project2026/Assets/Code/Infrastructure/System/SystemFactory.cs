using Entitas;
using VContainer;

namespace Code.Infrastructure.Systems
{
    public class SystemFactory : ISystemFactory
    {
        private readonly IObjectResolver _objectResolver;

        public SystemFactory(IObjectResolver objectResolver)
        {
            _objectResolver = objectResolver;
        }

        public T Create<T>() where T : ISystem =>
          _objectResolver.Resolve<T>();

        public T Create<T>(params object[] args) where T : ISystem =>
          _objectResolver.Resolve<T>(args);
    }
}