namespace Code.Infrastructure.View.Registrars
{
    public abstract class TownEntityComponentRegistrar : TownEntityDependant, IEntityComponentRegistrar
    {
        public abstract void RegisterComponents();
        public abstract void UnregisterComponents();
    }
}
