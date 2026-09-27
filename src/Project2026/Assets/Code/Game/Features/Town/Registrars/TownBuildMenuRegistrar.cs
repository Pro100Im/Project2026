using Code.Infrastructure.View.Registrars;
using Code.Meta.Features.Town;
using UnityEngine;

namespace Code.Game.Features.Town.Registrars
{
    public class TownBuildMenuRegistrar : TownEntityComponentRegistrar
    {
        [SerializeField] private TownBuildMenu _townBuildMenu;

        public override void RegisterComponents()
        {
            Entity.AddTownBuildMenu(_townBuildMenu);
        }

        public override void UnregisterComponents()
        {
            if (Entity.hasTownBuildMenu)
                Entity.RemoveTownBuildMenu();

            Entity.isTownBuildMenuOpened = false;
        }
    }
}
