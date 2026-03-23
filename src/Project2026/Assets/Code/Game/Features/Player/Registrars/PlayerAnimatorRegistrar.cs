using Code.Game.Features.Player.Service;
using Code.Infrastructure.View.Registrars;
using UnityEngine;

namespace Code.Game.Features.Player.Registrars
{
    public class PlayerAnimatorRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private PlayerAnimatorService _animatorService;

        public override void RegisterComponents()
        {
            Entity.AddPlayerAnimator(_animatorService);
        }

        public override void UnregisterComponents()
        {
            if (Entity.hasPlayerAnimator)
                Entity.RemovePlayerAnimator();
        }
    }
}