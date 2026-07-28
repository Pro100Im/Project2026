using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Tower.Systems
{
    public class TowerMenuCloseSystem : IExecuteSystem
    {
        private readonly IGroup<MetaEntity> _closeRequests;
        private readonly IGroup<MetaEntity> _towerMenus;
        private readonly IGroup<MetaEntity> _rangeViews;

        private readonly List<MetaEntity> _closeBuffer = new(4);

        public TowerMenuCloseSystem(MetaContext metaContext)
        {
            _closeRequests = metaContext.GetGroup(MetaMatcher
                .AllOf(MetaMatcher.TowerMenuCloseRequest)
                .NoneOf(MetaMatcher.Destructed));

            _towerMenus = metaContext.GetGroup(MetaMatcher.TowerMenu);
            _rangeViews = metaContext.GetGroup(MetaMatcher.UnitRangeView);
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
            {
                var request = requests[i];

                if (request.hasTargetId)
                    Deselect(request.targetId.Value);

                request.isDestructed = true;
            }
        }

        private void Deselect(int targetId)
        {
            var rangeView = _rangeViews.GetSingleEntity();

            if (rangeView == null || !rangeView.hasTargetId || rangeView.targetId.Value != targetId)
                return;

            if (rangeView.isUnitRangeShowed)
            {
                rangeView.unitRangeView.Value.HideRangeView();
                rangeView.isUnitRangeShowed = false;
            }

            rangeView.RemoveTargetId();
        }
    }
}
