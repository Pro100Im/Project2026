using Code.Infrastructure.View.Registrars;
using UnityEngine;

namespace Code.Game.Features.Tower.Registrars
{
    public class CannonRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private UnityEngine.Animator _cannonAnimator;
        [SerializeField] private SpriteRenderer _cannonSpriteRenderer;

        public override void RegisterComponents()
        {
            Entity.AddCannonAnimator(_cannonAnimator);
            Entity.AddCannonSpriteRenderer(_cannonSpriteRenderer);
        }

        public override void UnregisterComponents()
        {
            if (Entity.hasCannonAnimator)
                Entity.RemoveCannonAnimator();

            if (Entity.hasCannonSpriteRenderer)
                Entity.RemoveCannonSpriteRenderer();
        }
    }
}
