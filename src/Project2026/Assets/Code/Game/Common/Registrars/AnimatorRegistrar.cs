using Code.Infrastructure.View.Registrars;
using UnityEngine;

namespace Code.Game.Common.Registrars
{
    public class AnimatorRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private Animator _animator;

        public override void RegisterComponents()
        {
            Entity.AddAnimator(_animator);
        }

        public override void UnregisterComponents()
        {
            if (Entity.hasAnimator)
                Entity.RemoveAnimator();
        }
    }
}