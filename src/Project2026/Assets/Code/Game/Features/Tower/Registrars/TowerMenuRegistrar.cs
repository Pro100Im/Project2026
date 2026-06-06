using Code.Game.Common.Entity;
using Code.Meta.Features.Game;
using UnityEngine;

namespace Code.Game.Features.Tower.Registrars
{
    public class TowerMenuRegistrar : MonoBehaviour
    {
        [SerializeField] private TowerMenu _towerMenu;

        private void Awake()
        {
            var entity = CreateMetaEntity.Empty();
            entity.AddTowerMenu(_towerMenu);
        }

        //public override void RegisterComponents()
        //{
        //    var entity = CreateMetaEntity.Empty();
        //    entity.AddTowerMenu(_towerMenu);
        //}

        //public override void UnregisterComponents()
        //{
        //    //Entity.isPlayer = false;
        //    //Entity.isTowerPlace = false;
        //}
    }
}