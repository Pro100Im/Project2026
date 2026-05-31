using Code.Infrastructure.View.Registrars;
using Code.Meta.Features.Game;
using Entitas;
using UnityEngine;

namespace Code.Game.Features.Tower.Registrars
{
    public class TowerMenuRegistrar : EntityComponentRegistrar
    {
        [SerializeField] private TowerMenu _towerMenu;

        public override void RegisterComponents()
        {
            //Entity.
        }

        public override void UnregisterComponents()
        {
            //Entity.isPlayer = false;
            //Entity.isTowerPlace = false;
        }
    }
}