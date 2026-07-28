using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Tower.Systems
{
    public class TowerMenuCloseSystem : IExecuteSystem
    {
        private readonly IGroup<MetaEntity> _closeRequests;
        private readonly IGroup<MetaEntity> _towerMenus;

        private readonly List<MetaEntity> _closeBuffer = new(4);

        public TowerMenuCloseSystem(MetaContext metaContext)
        {
            _closeRequests = metaContext.GetGroup(MetaMatcher
                .AllOf(MetaMatcher.TowerMenuCloseRequest)
                .NoneOf(MetaMatcher.Destructed));

            _towerMenus = metaContext.GetGroup(MetaMatcher.TowerMenu);
        }

        public void Execute()
        {
            var requests = _closeRequests.GetEntities(_closeBuffer);

            if (requests.Count == 0)
                return;

            var menuEntity = _towerMenus.GetSingleEntity();

            if (menuEntity != null)
            {
                if (menuEntity.isTowerOpenBuildMenu)
                {
                    menuEntity.towerMenu.Value.CloseTowerBuilds();
                    menuEntity.isTowerOpenBuildMenu = false;
                }

                if (menuEntity.isTowerOpenUpgradeMenu)
                {
                    menuEntity.towerMenu.Value.CloseTowerUpgrades();
                    menuEntity.isTowerOpenUpgradeMenu = false;
                }
            }

            for (var i = 0; i < requests.Count; i++)
                requests[i].isDestructed = true;
        }
    }
}
