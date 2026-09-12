using System;
using System.Collections.Concurrent;
using System.Reflection;
using Entitas;
using VContainer;

namespace Code.Infrastructure.Systems
{
    public class SystemFactory : ISystemFactory
    {
        private static readonly ConcurrentDictionary<Type, ConstructorInfo> ConstructorCache = new();

        private readonly IObjectResolver _objectResolver;

        public SystemFactory(IObjectResolver objectResolver)
        {
            _objectResolver = objectResolver;
        }

        public T Create<T>() where T : ISystem =>
            CreateInstance<T>();

        public T Create<T>(params object[] args) where T : ISystem =>
            CreateInstance<T>(args);

        private T CreateInstance<T>(params object[] explicitArgs) where T : ISystem
        {
            var type = typeof(T);
            var constructor = ConstructorCache.GetOrAdd(type, ResolveConstructor);
            var parameters = constructor.GetParameters();
            var args = new object[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameterType = parameters[i].ParameterType;

                if (parameterType == typeof(ISystemFactory))
                {
                    args[i] = this;
                    continue;
                }

                if (TryGetExplicitArg(explicitArgs, parameterType, out var explicitArg))
                {
                    args[i] = explicitArg;
                    continue;
                }

                args[i] = _objectResolver.Resolve(parameterType);
            }

            return (T)constructor.Invoke(args);
        }

        private static ConstructorInfo ResolveConstructor(Type type)
        {
            var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

            if (constructors.Length == 0)
                throw new InvalidOperationException($"Type {type.Name} has no public constructors.");

            return constructors[0];
        }

        private static bool TryGetExplicitArg(object[] explicitArgs, Type parameterType, out object arg)
        {
            if (explicitArgs == null || explicitArgs.Length == 0)
            {
                arg = null;
                return false;
            }

            for (var i = 0; i < explicitArgs.Length; i++)
            {
                var candidate = explicitArgs[i];
                if (candidate == null)
                    continue;

                if (parameterType.IsInstanceOfType(candidate))
                {
                    arg = candidate;
                    return true;
                }
            }

            arg = null;
            return false;
        }
    }
}
