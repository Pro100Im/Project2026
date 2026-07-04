using Code.Game.Common.Entity;
using Code.Infrastructure.View.Registrars;
using Code.Meta.Features.Game;
using UnityEngine;

namespace Code.Game.Features.Tower.Registrars
{
    public class TowerMenuRegistrar : MetaEntityComponentRegistrar
    {
        [SerializeField] private TowerMenu _towerMenu;

        public override void RegisterComponents()
        {
            Entity.AddTowerMenu(_towerMenu);
        }

        public override void UnregisterComponents()
        {
            if(Entity.hasTowerMenu)
                Entity.RemoveTowerMenu();
        }
    }
}