using Entitas;

namespace Code.Game.Features.Tower.Systems
{
    public class TowerMenuAutoCloseSystem : IExecuteSystem
    {
        private readonly IGroup<MetaEntity> _towerMenus;
        private readonly IGroup<MetaEntity> _rangeViews;

        public TowerMenuAutoCloseSystem()
        {
            var metaContext = Contexts.sharedInstance.meta;

            _towerMenus = metaContext.GetGroup(MetaMatcher.TowerMenu);
            _rangeViews = metaContext.GetGroup(MetaMatcher.UnitRangeView);
        }

        public void Execute()
        {
            var menuEntity = _towerMenus.GetSingleEntity();

            if (menuEntity == null || !menuEntity.isTowerOpenBuildMenu && !menuEntity.isTowerOpenUpgradeMenu)
                return;

            var rangeView = _rangeViews.GetSingleEntity();

            if (rangeView != null
                && rangeView.hasTargetId
                && menuEntity.hasTargetId
                && rangeView.targetId.Value == menuEntity.targetId.Value)
                return;

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
    }
}
