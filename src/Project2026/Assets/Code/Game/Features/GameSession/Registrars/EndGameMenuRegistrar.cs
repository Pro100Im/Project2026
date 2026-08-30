using Code.Infrastructure.View.Registrars;
using Code.Meta.Features.Game;
using UnityEngine;

namespace Code.Game.Features.GameSession.Registrars
{
    public class EndGameMenuRegistrar : MetaEntityComponentRegistrar
    {
        [SerializeField] private EndGameMenu _endGameMenu;

        public override void RegisterComponents()
        {
            Entity.AddEndGameMenu(_endGameMenu);
        }

        public override void UnregisterComponents()
        {
            if (Entity.hasEndGameMenu)
                Entity.RemoveEndGameMenu();
        }
    }
}