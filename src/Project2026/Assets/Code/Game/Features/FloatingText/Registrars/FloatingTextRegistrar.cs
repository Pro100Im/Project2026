using Code.Infrastructure.View.Registrars;
using UnityEngine;

namespace Code.Game.Features.FloatingText.Registrars
{
    public class FloatingTextRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private FloatingTextView _floatingTextView;

        public override void RegisterComponents()
        {
            if (!Entity.hasFloatingText || _floatingTextView == null)
                return;

            _floatingTextView.Play(Entity.floatingText.Value, Entity.isHealFloatingText);
        }

        public override void UnregisterComponents()
        {
        }
    }
}
