using Code.Infrastructure.View.Registrars;
using Code.Meta.Features.Town;
using UnityEngine;

namespace Code.Game.Features.Town.Registrars
{
    public class TownResearchMenuRegistrar : TownEntityComponentRegistrar
    {
        [SerializeField] private TownResearchMenu _townResearchMenu;

        public override void RegisterComponents()
        {
            Entity.AddTownResearchMenu(_townResearchMenu);
        }

        public override void UnregisterComponents()
        {
            if (Entity.hasTownResearchMenu)
                Entity.RemoveTownResearchMenu();

            Entity.isTownResearchMenuOpened = false;
        }
    }
}
