using Code.Infrastructure.View.Registrars;
using Code.Meta.Features.Game;
using Entitas;
using UnityEngine;

namespace Code.Game.Features.Pause.Registrars
{
    public class PauseMenuRegistrar : MetaEntityComponentRegistrar
    {
        [SerializeField] private PauseMenu _pauseMenu;

        public override void RegisterComponents()
        {
            Entity.AddPauseMenu(_pauseMenu);
        }

        public override void UnregisterComponents()
        {
            if (Entity.hasPauseMenu)
                Entity.RemovePauseMenu();
        }
    }
}