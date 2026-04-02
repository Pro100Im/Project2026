using Code.Infrastructure.View.Registrars;
using UnityEngine;

namespace Code.Game.Features.Player.Registrars
{
    public class PlayerCastleRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private int _health;

        public override void RegisterComponents()
        {
            Entity.isPlayer = true;
            Entity.isTargetable = true;
            Entity.AddMaxHealth(_health);
            Entity.AddCurrentHealth(_health);
        }

        public override void UnregisterComponents()
        {
            Entity.RemoveAllComponents();
        }
    }
}