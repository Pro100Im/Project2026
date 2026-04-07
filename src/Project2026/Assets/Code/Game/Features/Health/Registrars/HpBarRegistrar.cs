using Code.Infrastructure.View.Registrars;
using UnityEngine;

namespace Code.Game.Features.Health.Registrars
{
    public class HpBarRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private HpBarView _hpBarView;

        public override void RegisterComponents()
        {
            Entity.AddHpBar(_hpBarView);
        }

        public override void UnregisterComponents()
        {
            if (Entity.hasHpBar)
                Entity.RemoveHpBar();
        }
    }
}