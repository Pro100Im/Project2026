using Code.Infrastructure.View.Registrars;
using UnityEngine;

namespace Code.Game.Features.Town.Registrars
{
    public class TownExchequerRegistrar : TownEntityComponentRegistrar
    {
        [SerializeField] private TownExchequerView _townExchequerView;

        public override void RegisterComponents()
        {
            Entity.AddTownExchequer(_townExchequerView);
            Entity.isTownResources = true;
            Entity.AddTownGold(0);
            Entity.AddTownStone(0);
            Entity.AddTownCrystal(0);
            Entity.AddTownWood(0);
            Entity.AddTownKnowledge(0);
        }

        public override void UnregisterComponents()
        {
            if (Entity.hasTownExchequer)
                Entity.RemoveTownExchequer();

            Entity.isTownResources = false;

            if (Entity.hasTownGold)
                Entity.RemoveTownGold();
            if (Entity.hasTownStone)
                Entity.RemoveTownStone();
            if (Entity.hasTownCrystal)
                Entity.RemoveTownCrystal();
            if (Entity.hasTownWood)
                Entity.RemoveTownWood();
            if (Entity.hasTownKnowledge)
                Entity.RemoveTownKnowledge();
        }
    }
}
