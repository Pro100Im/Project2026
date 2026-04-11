using Code.Infrastructure.View.Registrars;

namespace Code.Game.Features.Tower.Registrars
{
    public class TowerRegistrar : EntityComponentRegistrar
    {
        public override void RegisterComponents()
        {
            Entity.isPlayer = true;
            Entity.isTowerPlace = true;
        }

        public override void UnregisterComponents()
        {
            Entity.isPlayer = false;
            Entity.isTowerPlace = false;
        }
    }
}