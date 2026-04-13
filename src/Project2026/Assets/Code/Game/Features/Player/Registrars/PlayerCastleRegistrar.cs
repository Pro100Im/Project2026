using Code.Game.Common.Entity;
using Code.Infrastructure.View;
using Code.Infrastructure.View.Registrars;
using System;
using UnityEngine;

namespace Code.Game.Features.Player.Registrars
{
    [Serializable]
    public class PlayerCastleRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private int _health;
        [SerializeField] private Vector3 _hpBarOffset;
        [SerializeField] private EntityBehaviour _hpBar;

        public override void RegisterComponents()
        {
            Entity.isPlayer = true;
            Entity.isTargetable = true;
            Entity.isPlayerCastle = true;
            Entity.AddMaxHealth(_health);
            Entity.AddCurrentHealth(_health);

            var hpBar = CreateGameEntity.Empty();
            hpBar.AddViewPrefab(_hpBar);
            hpBar.AddSpawnPosition(transform.position + _hpBarOffset);
            hpBar.AddMovementOffset(_hpBarOffset);
            hpBar.AddOwnerId(Entity.id.Value);
            hpBar.AddCurrentHealth(_health);
        }

        public override void UnregisterComponents()
        {
            Entity.RemoveAllComponents();
        }
    }
}