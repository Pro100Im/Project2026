using Code.Infrastructure.View;

namespace Code.Infrastructure.View.Registrars
{
    public abstract class MetaEntityComponentRegistrar : MetaEntityDependant, IEntityComponentRegistrar
    {
        public abstract void RegisterComponents();
        public abstract void UnregisterComponents();
    }
}
