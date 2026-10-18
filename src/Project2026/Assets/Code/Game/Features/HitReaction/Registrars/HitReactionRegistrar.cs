using Code.Infrastructure.View.Registrars;
using UnityEngine;

namespace Code.Game.Features.HitReaction.Registrars
{
    public class HitReactionRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private HitReactionView _hitReactionView;

        public override void RegisterComponents()
        {
            Entity.AddHitReaction(_hitReactionView);
        }

        public override void UnregisterComponents()
        {
            if (_hitReactionView != null)
                _hitReactionView.Stop();

            if (Entity.hasHitReactionRequest)
                Entity.RemoveHitReactionRequest();

            if (Entity.hasHitReaction)
                Entity.RemoveHitReaction();
        }
    }
}
