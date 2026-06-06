using Code.Game.Common.Entity;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Tower.Systems
{
    public class TowerMenuSystem : ReactiveSystem<InputEntity>
    {
        private readonly IGroup<MetaEntity> _towerMenu;

        public TowerMenuSystem(InputContext inputContext, MetaContext metaContext)
            : base(inputContext)
        {
            _towerMenu = metaContext.GetGroup(MetaMatcher.TowerMenu);
        }

        protected override ICollector<InputEntity> GetTrigger(IContext<InputEntity> context) =>
            context.CreateCollector(InputMatcher
                .AllOf(
                InputMatcher.Input
                ));

        protected override bool Filter(InputEntity entity) => entity.isInput;

        protected override void Execute(List<InputEntity> entities)
        {
            var menuEntity = _towerMenu.GetSingleEntity();

            if (menuEntity == null)
                return;

            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];

                if (entity.hasTargetId)
                {
                    var targetEntity = GetGameEntityById.Get(entity.targetId.Value);

                    if (targetEntity.isTowerPlace)
                    {
                        if (menuEntity.isTowerOpenUpgradeMenu)
                            menuEntity.towerMenu.Value.CloseTowerUpgrades();

                        if (menuEntity.isTowerOpenBuildMenu && menuEntity.targetId.Value == targetEntity.id.Value)
                        {
                            menuEntity.isTowerOpenBuildMenu = false;
                            menuEntity.towerMenu.Value.CloseTowerBuilds();
                        }
                        else
                        {
                            menuEntity.ReplaceTargetId(targetEntity.id.Value);
                            menuEntity.isTowerOpenBuildMenu = true;
                            menuEntity.towerMenu.Value.OpenTowerBuildMenu(entity.screenPointerInput.Value, targetEntity);
                        }
                    }
                    else if (targetEntity.isTower)
                    {
                        if (menuEntity.isTowerOpenBuildMenu)
                            menuEntity.towerMenu.Value.CloseTowerBuilds();

                        if (menuEntity.isTowerOpenUpgradeMenu && menuEntity.targetId.Value == targetEntity.id.Value)
                        {
                            menuEntity.isTowerOpenUpgradeMenu = false;
                            menuEntity.towerMenu.Value.CloseTowerUpgrades();
                        }
                        else if (targetEntity.hasTowerUpgrade)
                        {
                            menuEntity.ReplaceTargetId(targetEntity.id.Value);
                            menuEntity.isTowerOpenUpgradeMenu = true;
                            menuEntity.towerMenu.Value.OpenTowerUpgradeMenu(entity.screenPointerInput.Value, targetEntity);
                        }
                        else if (menuEntity.isTowerOpenUpgradeMenu)
                        {
                            menuEntity.towerMenu.Value.CloseTowerUpgrades();
                            menuEntity.isTowerOpenUpgradeMenu = false;

                            if (menuEntity.hasTargetId)
                                menuEntity.RemoveTargetId();
                        }
                    }

                    entity.isDestructed = true;
                }
                else
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

                    if (menuEntity.hasTargetId)
                        menuEntity.RemoveTargetId();
                }

                entity.isDestructed = true;
            }
        }
    }
}